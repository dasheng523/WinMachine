using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MachineOrchestration.ControlBoards.Abstractions;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Dsl.Ast;
using MachineOrchestration.Dsl.Interpreter;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Automation.Executor;

/// <summary>
/// 自动化执行器实现（副作用边界）
/// 集成纯函数解释器和控制板，执行 DSL 程序
/// </summary>
/// <remarks>
/// 验证：需求 15.1-15.6、24.1-24.6
/// </remarks>
public sealed class AutomationExecutor : IDslExecutor, IDisposable
{
    private readonly IDslInterpreter _interpreter;
    private readonly IControlBoard _controlBoard;
    private readonly ILogger<AutomationExecutor> _logger;
    private readonly BehaviorSubject<ExecutionState> _stateSubject;
    private readonly SemaphoreSlim _executionLock;
    
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _executionTask;
    private bool _isDisposed;
    
    /// <summary>
    /// 创建自动化执行器
    /// </summary>
    public AutomationExecutor(
        IDslInterpreter interpreter,
        IControlBoard controlBoard,
        ILogger<AutomationExecutor> logger)
    {
        _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
        _controlBoard = controlBoard ?? throw new ArgumentNullException(nameof(controlBoard));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _stateSubject = new BehaviorSubject<ExecutionState>(
            ExecutionState.Initial(MachineState.Empty));
        _executionLock = new SemaphoreSlim(1, 1);
    }
    
    /// <inheritdoc />
    public IObservable<ExecutionState> ExecutionStateStream => _stateSubject.AsObservable();
    
    /// <inheritdoc />
    public bool IsRunning => _executionTask != null && !_executionTask.IsCompleted;
    
    /// <inheritdoc />
    public async Task<Either<ExecutionError, Unit>> Execute(
        Ast ast,
        ExecutionState initialState,
        CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(AutomationExecutor));
        
        // 确保只有一个执行在运行
        if (!await _executionLock.WaitAsync(0, cancellationToken))
        {
            return Left<ExecutionError, Unit>(
                new ExecutionError.InvalidStateTransition(
                    "Executor is already running",
                    "Running",
                    "Execute"));
        }
        
        try
        {
            _logger.LogInformation("Starting automation execution");
            
            // 初始化控制板
            var initResult = await _controlBoard.Initialize();
            if (initResult.IsLeft)
            {
                var error = initResult.LeftAsEnumerable().Head();
                _logger.LogError("Failed to initialize control board: {Error}", error);
                return Left<ExecutionError, Unit>(
                    new ExecutionError.HardwareError(
                        $"Control board initialization failed: {error}",
                        None));
            }
            
            // 创建取消令牌源
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            // 发布初始状态
            _stateSubject.OnNext(initialState);
            
            // 开始执行循环
            var executionTask = ExecutionLoop(ast, initialState, _cancellationTokenSource.Token);
            _executionTask = executionTask;
            
            var result = await executionTask;
            
            _logger.LogInformation("Automation execution completed");
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Automation execution cancelled");
            return Left<ExecutionError, Unit>(
                new ExecutionError.Unknown("Execution cancelled"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during automation execution");
            return Left<ExecutionError, Unit>(
                new ExecutionError.Unknown($"Unexpected error: {ex.Message}"));
        }
        finally
        {
            _executionLock.Release();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _executionTask = null;
        }
    }
    
    /// <inheritdoc />
    public async Task<Either<ExecutionError, Unit>> Stop()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(AutomationExecutor));
        
        _logger.LogInformation("Stopping automation execution");
        
        // 取消执行
        _cancellationTokenSource?.Cancel();
        
        // 等待执行任务完成
        if (_executionTask != null)
        {
            try
            {
                await _executionTask;
            }
            catch (OperationCanceledException)
            {
                // 预期的取消异常
            }
        }
        
        // 执行紧急停止
        var stopResult = await EmergencyStop();
        
        return stopResult;
    }
    
    /// <summary>
    /// 执行循环（主执行逻辑）
    /// </summary>
    private async Task<Either<ExecutionError, Unit>> ExecutionLoop(
        Ast ast,
        ExecutionState currentState,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // 检查是否完成
            if (_interpreter.IsComplete(currentState))
            {
                _logger.LogInformation("Execution completed successfully");
                _stateSubject.OnNext(currentState);
                return Right<ExecutionError, Unit>(unit);
            }
            
            // 执行一步（纯函数状态转换）
            var stepResult = _interpreter.Step(currentState, ast);
            
            if (stepResult.IsLeft)
            {
                var error = stepResult.LeftAsEnumerable().Head();
                _logger.LogError("Execution error: {Error}", error.GetMessage());
                
                // 执行紧急停止
                await EmergencyStop();
                
                // 标记状态为错误
                var errorState = currentState.MarkError(error.GetMessage());
                _stateSubject.OnNext(errorState);
                
                return Left<ExecutionError, Unit>(error);
            }
            
            var newState = stepResult.RightAsEnumerable().Head();
            
            // 执行副作用（发送命令到控制板）
            var commandResult = await ExecuteSideEffects(currentState, newState, cancellationToken);
            
            if (commandResult.IsLeft)
            {
                var error = commandResult.LeftAsEnumerable().Head();
                _logger.LogError("Command execution error: {Error}", error.GetMessage());
                
                // 执行紧急停止
                await EmergencyStop();
                
                // 标记状态为错误
                var errorState = newState.MarkError(error.GetMessage());
                _stateSubject.OnNext(errorState);
                
                return Left<ExecutionError, Unit>(error);
            }
            
            // 发布新状态
            _stateSubject.OnNext(newState);
            
            // 更新当前状态
            currentState = newState;
            
            // 短暂延迟以避免忙等待
            await Task.Delay(10, cancellationToken);
        }
        
        _logger.LogInformation("Execution cancelled");
        return Left<ExecutionError, Unit>(
            new ExecutionError.Unknown("Execution cancelled"));
    }
    
    /// <summary>
    /// 执行副作用（发送命令到控制板）
    /// </summary>
    private async Task<Either<ExecutionError, Unit>> ExecuteSideEffects(
        ExecutionState oldState,
        ExecutionState newState,
        CancellationToken cancellationToken)
    {
        // 比较状态变化，找出需要执行的命令
        var commands = DetectStateChanges(oldState.MachineState, newState.MachineState);
        
        // 执行所有命令
        foreach (var (entityId, partState) in commands)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            
            var result = await ExecuteCommand(entityId, partState);
            
            if (result.IsLeft)
            {
                return result;
            }
        }
        
        return Right<ExecutionError, Unit>(unit);
    }
    
    /// <summary>
    /// 检测状态变化
    /// </summary>
    private Seq<(EntityId, PartState)> DetectStateChanges(
        MachineState oldState,
        MachineState newState)
    {
        var changes = Seq<(EntityId, PartState)>();
        
        // 遍历新状态中的所有零件
        foreach (var (entityId, newPartState) in newState.PartStates)
        {
            var oldPartStateOpt = oldState.GetPartState(entityId);
            
            // 检查状态是否改变
            var hasChanged = oldPartStateOpt.Match(
                Some: oldPartState => !oldPartState.Equals(newPartState),
                None: () => true);
            
            if (hasChanged)
            {
                changes = changes.Add((entityId, newPartState));
            }
        }
        
        return changes;
    }
    
    /// <summary>
    /// 执行单个命令
    /// </summary>
    private async Task<Either<ExecutionError, Unit>> ExecuteCommand(
        EntityId entityId,
        PartState partState)
    {
        try
        {
            return partState switch
            {
                PartState.Motor motor => await ExecuteMotorCommand(entityId, motor),
                PartState.Actuator actuator => await ExecuteActuatorCommand(entityId, actuator),
                PartState.Sensor sensor => Right<ExecutionError, Unit>(unit), // 传感器不需要命令
                _ => Left<ExecutionError, Unit>(
                    new ExecutionError.InvalidStateTransition(
                        $"Unknown part state type: {partState.GetType().Name}",
                        "ExecuteCommand",
                        "Unknown"))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command for entity {EntityId}", entityId);
            return Left<ExecutionError, Unit>(
                new ExecutionError.HardwareError(
                    $"Command execution failed: {ex.Message}",
                    Some(entityId)));
        }
    }
    
    /// <summary>
    /// 执行电机命令
    /// </summary>
    private async Task<Either<ExecutionError, Unit>> ExecuteMotorCommand(
        EntityId entityId,
        PartState.Motor motor)
    {
        var motorId = new MotorId(entityId.Value);
        
        // 根据电机状态确定动作
        MotorAction action;
        
        if (motor.IsHomed && motor.IsMoving)
        {
            action = MotorAction.Home.Instance;
        }
        else if (motor.IsMoving)
        {
            action = new MotorAction.MoveTo(motor.CurrentPosition, motor.CurrentSpeed);
        }
        else
        {
            action = MotorAction.Stop.Instance;
        }
        
        _logger.LogDebug("Sending motor command: {Action} to {MotorId}", action, motorId);
        
        var result = await _controlBoard.SendMotorCommand(motorId, action);
        
        return result.MapLeft<ExecutionError>(error =>
            new ExecutionError.HardwareError(
                $"Motor command failed: {error}",
                Some(entityId)));
    }
    
    /// <summary>
    /// 执行执行器命令
    /// </summary>
    private async Task<Either<ExecutionError, Unit>> ExecuteActuatorCommand(
        EntityId entityId,
        PartState.Actuator actuator)
    {
        var actuatorId = new ActuatorId(entityId.Value);
        
        // 根据执行器状态确定动作
        ActuatorAction? action = actuator.State switch
        {
            ActuatorStateValue.Extended => ActuatorAction.Extend.Instance,
            ActuatorStateValue.Retracted => ActuatorAction.Retract.Instance,
            ActuatorStateValue.Closed => ActuatorAction.Close.Instance,
            ActuatorStateValue.Opened => ActuatorAction.Open.Instance,
            ActuatorStateValue.Suctioning => ActuatorAction.Suction.Instance,
            ActuatorStateValue.Normal => ActuatorAction.Normal.Instance,
            ActuatorStateValue.On => ActuatorAction.On.Instance,
            ActuatorStateValue.Off => ActuatorAction.Off.Instance,
            _ => null
        };
        
        if (action == null)
        {
            _logger.LogWarning("Unknown actuator state: {State}", actuator.State);
            return Right<ExecutionError, Unit>(unit);
        }
        
        _logger.LogDebug("Sending actuator command: {Action} to {ActuatorId}", action, actuatorId);
        
        var result = await _controlBoard.SendActuatorCommand(actuatorId, action);
        
        return result.MapLeft<ExecutionError>(error =>
            new ExecutionError.HardwareError(
                $"Actuator command failed: {error}",
                Some(entityId)));
    }
    
    /// <summary>
    /// 紧急停止（错误恢复）
    /// </summary>
    private async Task<Either<ExecutionError, Unit>> EmergencyStop()
    {
        _logger.LogWarning("Executing emergency stop");
        
        try
        {
            var result = await _controlBoard.EmergencyStop();
            
            if (result.IsLeft)
            {
                var error = result.LeftAsEnumerable().Head();
                _logger.LogError("Emergency stop failed: {Error}", error);
                return Left<ExecutionError, Unit>(
                    new ExecutionError.HardwareError(
                        $"Emergency stop failed: {error}",
                        None));
            }
            
            _logger.LogInformation("Emergency stop completed successfully");
            return Right<ExecutionError, Unit>(unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during emergency stop");
            return Left<ExecutionError, Unit>(
                new ExecutionError.HardwareError(
                    $"Emergency stop error: {ex.Message}",
                    None));
        }
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;
        
        _isDisposed = true;
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        
        _stateSubject?.Dispose();
        _executionLock?.Dispose();
    }
}
