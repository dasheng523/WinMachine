using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.ControlBoards.Abstractions;
using MachineOrchestration.ControlBoards.Sdk;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.ControlBoards.Implementations;

/// <summary>
/// 雷赛控制板实现
/// 集成雷赛 SDK，提供统一的控制板接口
/// 包含错误处理和重试逻辑
/// </summary>
public sealed class LeiSaiBoard : IControlBoard, IDisposable
{
    private readonly ILeiSaiSdk _sdk;
    private readonly LeiSaiBoardConfig _config;
    private readonly BehaviorSubject<ControlBoardState> _stateSubject;
    private readonly ConcurrentDictionary<MotorId, int> _motorIdMap;
    private readonly ConcurrentDictionary<ActuatorId, int> _actuatorIdMap;
    private readonly ConcurrentDictionary<SensorId, int> _sensorIdMap;
    private readonly ConcurrentDictionary<StateSensorId, int> _stateSensorIdMap;
    private readonly Timer _stateUpdateTimer;
    private bool _isInitialized;
    private bool _disposed;

    public LeiSaiBoard(ILeiSaiSdk sdk, LeiSaiBoardConfig config)
    {
        _sdk = sdk ?? throw new ArgumentNullException(nameof(sdk));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _stateSubject = new BehaviorSubject<ControlBoardState>(ControlBoardState.Initial());
        _motorIdMap = new ConcurrentDictionary<MotorId, int>();
        _actuatorIdMap = new ConcurrentDictionary<ActuatorId, int>();
        _sensorIdMap = new ConcurrentDictionary<SensorId, int>();
        _stateSensorIdMap = new ConcurrentDictionary<StateSensorId, int>();
        
        // 设置状态更新定时器
        _stateUpdateTimer = new Timer(
            _ => UpdateStateAsync().Wait(),
            null,
            Timeout.Infinite,
            Timeout.Infinite);
    }

    public IObservable<ControlBoardState> StateStream => _stateSubject.AsObservable();

    public async Task<Either<ControlBoardError, Unit>> Initialize()
    {
        return await ExecuteWithRetry(async () =>
        {
            // 解析连接字符串（格式：IP:Port）
            var parts = _config.IpAddress.Split(':');
            var ipAddress = parts[0];
            var port = parts.Length > 1 ? int.Parse(parts[1]) : _config.Port;

            var connected = await _sdk.ConnectAsync(ipAddress, port);
            
            if (!connected)
            {
                return Left<ControlBoardError, Unit>(
                    new ControlBoardError.ConnectionError(
                        $"Failed to connect to LeiSai board at {ipAddress}:{port}"));
            }

            _isInitialized = true;
            
            // 启动状态更新定时器
            _stateUpdateTimer.Change(
                _config.StateUpdateInterval,
                _config.StateUpdateInterval);
            
            // 发布初始化状态
            await UpdateStateAsync();
            
            return Right<ControlBoardError, Unit>(unit);
        }, "Initialize");
    }

    public async Task<Either<ControlBoardError, Unit>> SendMotorCommand(
        MotorId motorId,
        MotorAction action)
    {
        if (!_isInitialized)
        {
            return Left<ControlBoardError, Unit>(new ControlBoardError.NotInitialized());
        }

        var axisId = _motorIdMap.GetOrAdd(motorId, _ => _motorIdMap.Count);
        
        return await ExecuteWithRetry(async () =>
        {
            var success = action switch
            {
                MotorAction.MoveTo(var position, var speed) =>
                    await _sdk.MoveMotorAsync(axisId, position, speed),
                
                MotorAction.RotateTo(var angle, var speed) =>
                    await _sdk.RotateMotorAsync(axisId, angle, speed),
                
                MotorAction.Home =>
                    await _sdk.HomeMotorAsync(axisId),
                
                MotorAction.Stop =>
                    await _sdk.StopMotorAsync(axisId),
                
                _ => throw new InvalidOperationException($"Unknown motor action: {action}")
            };

            if (!success)
            {
                return Left<ControlBoardError, Unit>(
                    new ControlBoardError.CommandFailed(
                        $"Motor command {action.GetType().Name}",
                        "SDK returned false"));
            }

            await UpdateStateAsync();
            return Right<ControlBoardError, Unit>(unit);
        },
        $"SendMotorCommand({motorId}, {action.GetType().Name})");
    }

    public async Task<Either<ControlBoardError, Unit>> SendActuatorCommand(
        ActuatorId actuatorId,
        ActuatorAction action)
    {
        if (!_isInitialized)
        {
            return Left<ControlBoardError, Unit>(new ControlBoardError.NotInitialized());
        }

        var portId = _actuatorIdMap.GetOrAdd(actuatorId, _ => _actuatorIdMap.Count);
        
        return await ExecuteWithRetry(async () =>
        {
            var state = action switch
            {
                ActuatorAction.Extend => true,
                ActuatorAction.Retract => false,
                ActuatorAction.Close => true,
                ActuatorAction.Open => false,
                ActuatorAction.Suction => true,
                ActuatorAction.Normal => false,
                ActuatorAction.On => true,
                ActuatorAction.Off => false,
                _ => throw new InvalidOperationException($"Unknown actuator action: {action}")
            };

            var success = await _sdk.SetOutputAsync(portId, state);
            
            if (!success)
            {
                return Left<ControlBoardError, Unit>(
                    new ControlBoardError.CommandFailed(
                        $"Actuator command {action.GetType().Name}",
                        "SDK returned false"));
            }

            await UpdateStateAsync();
            return Right<ControlBoardError, Unit>(unit);
        },
        $"SendActuatorCommand({actuatorId}, {action.GetType().Name})");
    }

    public async Task<Either<ControlBoardError, SensorReading>> ReadSensor(SensorId sensorId)
    {
        if (!_isInitialized)
        {
            return Left<ControlBoardError, SensorReading>(new ControlBoardError.NotInitialized());
        }

        var channelId = _sensorIdMap.GetOrAdd(sensorId, _ => _sensorIdMap.Count);
        
        return await ExecuteWithRetry(async () =>
        {
            var value = await _sdk.ReadAnalogInputAsync(channelId);
            var reading = new SensorReading.Pressure(value, "Pa");
            return Right<ControlBoardError, SensorReading>(reading);
        },
        $"ReadSensor({sensorId})");
    }

    public async Task<Either<ControlBoardError, bool>> ReadStateSensor(StateSensorId stateSensorId)
    {
        if (!_isInitialized)
        {
            return Left<ControlBoardError, bool>(new ControlBoardError.NotInitialized());
        }

        var portId = _stateSensorIdMap.GetOrAdd(stateSensorId, _ => _stateSensorIdMap.Count);
        
        return await ExecuteWithRetry(async () =>
        {
            var state = await _sdk.ReadInputAsync(portId);
            return Right<ControlBoardError, bool>(state);
        },
        $"ReadStateSensor({stateSensorId})");
    }

    public async Task<Either<ControlBoardError, Unit>> EmergencyStop()
    {
        if (!_isInitialized)
        {
            return Left<ControlBoardError, Unit>(new ControlBoardError.NotInitialized());
        }

        try
        {
            var success = await _sdk.EmergencyStopAllAsync();
            
            if (!success)
            {
                return Left<ControlBoardError, Unit>(
                    new ControlBoardError.CommandFailed(
                        "EmergencyStop",
                        "SDK returned false"));
            }

            await UpdateStateAsync();
            return Right<ControlBoardError, Unit>(unit);
        }
        catch (Exception ex)
        {
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.CommandFailed(
                    "EmergencyStop",
                    "Exception occurred",
                    ex));
        }
    }

    /// <summary>
    /// 使用指数退避重试执行操作
    /// </summary>
    private async Task<Either<ControlBoardError, T>> ExecuteWithRetry<T>(
        Func<Task<Either<ControlBoardError, T>>> operation,
        string operationName)
    {
        var attempt = 0;
        var delay = _config.InitialRetryDelay;
        Either<ControlBoardError, T> lastResult = Left<ControlBoardError, T>(
            new ControlBoardError.CommandFailed(operationName, "Not attempted"));

        while (attempt < _config.MaxRetries)
        {
            try
            {
                var result = await operation();
                
                // 如果成功，立即返回
                if (result.IsRight)
                {
                    return result;
                }
                
                // 如果失败，保存结果并重试
                lastResult = result;
                attempt++;
                
                if (attempt >= _config.MaxRetries)
                {
                    return lastResult;
                }

                // 指数退避
                await Task.Delay(delay);
                delay *= 2;
            }
            catch (Exception ex)
            {
                attempt++;
                
                if (attempt >= _config.MaxRetries)
                {
                    return Left<ControlBoardError, T>(
                        new ControlBoardError.CommandFailed(
                            operationName,
                            $"Failed after {attempt} attempts",
                            ex));
                }

                // 指数退避
                await Task.Delay(delay);
                delay *= 2;
            }
        }

        return lastResult;
    }

    /// <summary>
    /// 更新控制板状态
    /// </summary>
    private async Task UpdateStateAsync()
    {
        try
        {
            if (!_sdk.IsConnected)
            {
                return;
            }

            var motorStates = HashMap<MotorId, MotorState>();
            foreach (var (motorId, axisId) in _motorIdMap)
            {
                try
                {
                    var position = await _sdk.GetMotorPositionAsync(axisId);
                    var speed = await _sdk.GetMotorSpeedAsync(axisId);
                    var isMoving = await _sdk.IsMotorMovingAsync(axisId);
                    
                    motorStates = motorStates.Add(motorId, new MotorState(
                        CurrentPosition: position,
                        CurrentSpeed: speed,
                        IsMoving: isMoving,
                        IsHomed: position == 0f && !isMoving));
                }
                catch
                {
                    // 忽略单个电机状态读取失败
                }
            }

            var actuatorStates = HashMap<ActuatorId, ActuatorState>();
            foreach (var (actuatorId, portId) in _actuatorIdMap)
            {
                try
                {
                    var state = await _sdk.ReadInputAsync(portId);
                    actuatorStates = actuatorStates.Add(actuatorId, new ActuatorState(
                        CurrentState: state ? "Active" : "Inactive",
                        IsActive: state));
                }
                catch
                {
                    // 忽略单个执行器状态读取失败
                }
            }

            var newState = new ControlBoardState(
                Timestamp: DateTime.UtcNow,
                IsInitialized: _isInitialized,
                MotorStates: motorStates,
                ActuatorStates: actuatorStates,
                SensorReadings: HashMap<SensorId, Option<SensorReading>>(),
                StateSensorStates: HashMap<StateSensorId, bool>());

            _stateSubject.OnNext(newState);
        }
        catch
        {
            // 忽略状态更新失败
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _stateUpdateTimer?.Dispose();
        _sdk.DisconnectAsync().Wait();
        _stateSubject?.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// 雷赛控制板配置
/// </summary>
public sealed record LeiSaiBoardConfig(
    string IpAddress,
    int Port,
    int MaxRetries,
    int InitialRetryDelay,
    int StateUpdateInterval,
    float DefaultHomeSpeed);
