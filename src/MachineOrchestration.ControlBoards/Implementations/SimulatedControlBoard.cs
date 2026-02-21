using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.ControlBoards.Abstractions;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.ControlBoards.Implementations;

/// <summary>
/// 模拟控制板实现
/// 用于测试和开发，模拟真实控制板的行为
/// </summary>
public sealed class SimulatedControlBoard : IControlBoard, IDisposable
{
    private readonly SimulatedControlBoardConfig _config;
    private readonly BehaviorSubject<ControlBoardState> _stateSubject;
    private readonly ConcurrentDictionary<MotorId, MotorState> _motorStates;
    private readonly ConcurrentDictionary<ActuatorId, ActuatorState> _actuatorStates;
    private readonly ConcurrentDictionary<SensorId, Option<SensorReading>> _sensorReadings;
    private readonly ConcurrentDictionary<StateSensorId, bool> _stateSensorStates;
    private readonly Random _random;
    private bool _isInitialized;
    private bool _disposed;

    public SimulatedControlBoard(SimulatedControlBoardConfig? config = null)
    {
        _config = config ?? SimulatedControlBoardConfig.Default;
        _motorStates = new ConcurrentDictionary<MotorId, MotorState>();
        _actuatorStates = new ConcurrentDictionary<ActuatorId, ActuatorState>();
        _sensorReadings = new ConcurrentDictionary<SensorId, Option<SensorReading>>();
        _stateSensorStates = new ConcurrentDictionary<StateSensorId, bool>();
        _random = new Random();
        _isInitialized = false;
        
        _stateSubject = new BehaviorSubject<ControlBoardState>(GetCurrentState());
    }

    public IObservable<ControlBoardState> StateStream => _stateSubject.AsObservable();

    public async Task<Either<ControlBoardError, Unit>> Initialize()
    {
        if (_disposed)
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.ConnectionError("Control board has been disposed"));

        // 模拟初始化延迟
        await Task.Delay(_config.InitializationDelay);

        _isInitialized = true;
        PublishState();
        
        return Right<ControlBoardError, Unit>(unit);
    }

    public async Task<Either<ControlBoardError, Unit>> SendMotorCommand(
        MotorId motorId,
        MotorAction action)
    {
        if (!_isInitialized)
            return Left<ControlBoardError, Unit>(new ControlBoardError.NotInitialized());

        if (_disposed)
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.ConnectionError("Control board has been disposed"));

        try
        {
            // 模拟命令延迟
            await Task.Delay(_config.CommandDelay);

            var currentState = _motorStates.GetOrAdd(motorId, 
                _ => new MotorState(0f, 0f, false, false));

            var newState = action switch
            {
                MotorAction.MoveTo moveTo => await SimulateMotorMoveTo(motorId, currentState, moveTo),
                MotorAction.RotateTo rotateTo => await SimulateMotorRotateTo(motorId, currentState, rotateTo),
                MotorAction.Home => await SimulateMotorHome(motorId, currentState),
                MotorAction.Stop => currentState with { IsMoving = false, CurrentSpeed = 0f },
                _ => currentState
            };

            _motorStates[motorId] = newState;
            PublishState();

            return Right<ControlBoardError, Unit>(unit);
        }
        catch (Exception ex)
        {
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.CommandFailed($"Motor command {action}", ex.Message, ex));
        }
    }

    public async Task<Either<ControlBoardError, Unit>> SendActuatorCommand(
        ActuatorId actuatorId,
        ActuatorAction action)
    {
        if (!_isInitialized)
            return Left<ControlBoardError, Unit>(new ControlBoardError.NotInitialized());

        if (_disposed)
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.ConnectionError("Control board has been disposed"));

        try
        {
            // 模拟命令延迟
            await Task.Delay(_config.CommandDelay);

            var currentState = _actuatorStates.GetOrAdd(actuatorId,
                _ => new ActuatorState("Idle", false));

            var newState = action switch
            {
                ActuatorAction.Extend => await SimulateActuatorAction(actuatorId, "Extended", true),
                ActuatorAction.Retract => await SimulateActuatorAction(actuatorId, "Retracted", false),
                ActuatorAction.Close => await SimulateActuatorAction(actuatorId, "Closed", true),
                ActuatorAction.Open => await SimulateActuatorAction(actuatorId, "Opened", false),
                ActuatorAction.Suction => await SimulateActuatorAction(actuatorId, "Suction", true),
                ActuatorAction.Normal => await SimulateActuatorAction(actuatorId, "Normal", false),
                ActuatorAction.On => await SimulateActuatorAction(actuatorId, "On", true),
                ActuatorAction.Off => await SimulateActuatorAction(actuatorId, "Off", false),
                _ => currentState
            };

            _actuatorStates[actuatorId] = newState;
            PublishState();

            return Right<ControlBoardError, Unit>(unit);
        }
        catch (Exception ex)
        {
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.CommandFailed($"Actuator command {action}", ex.Message, ex));
        }
    }

    public async Task<Either<ControlBoardError, SensorReading>> ReadSensor(SensorId sensorId)
    {
        if (!_isInitialized)
            return Left<ControlBoardError, SensorReading>(new ControlBoardError.NotInitialized());

        if (_disposed)
            return Left<ControlBoardError, SensorReading>(
                new ControlBoardError.ConnectionError("Control board has been disposed"));

        try
        {
            // 模拟读取延迟
            await Task.Delay(_config.SensorReadDelay);

            // 生成随机传感器读数
            var reading = GenerateRandomSensorReading();
            _sensorReadings[sensorId] = Some(reading);
            PublishState();

            return Right<ControlBoardError, SensorReading>(reading);
        }
        catch (Exception ex)
        {
            return Left<ControlBoardError, SensorReading>(
                new ControlBoardError.CommandFailed($"Read sensor {sensorId}", ex.Message, ex));
        }
    }

    public async Task<Either<ControlBoardError, bool>> ReadStateSensor(StateSensorId stateSensorId)
    {
        if (!_isInitialized)
            return Left<ControlBoardError, bool>(new ControlBoardError.NotInitialized());

        if (_disposed)
            return Left<ControlBoardError, bool>(
                new ControlBoardError.ConnectionError("Control board has been disposed"));

        try
        {
            // 模拟读取延迟
            await Task.Delay(_config.SensorReadDelay);

            // 生成随机布尔值
            var state = _random.Next(2) == 1;
            _stateSensorStates[stateSensorId] = state;
            PublishState();

            return Right<ControlBoardError, bool>(state);
        }
        catch (Exception ex)
        {
            return Left<ControlBoardError, bool>(
                new ControlBoardError.CommandFailed($"Read state sensor {stateSensorId}", ex.Message, ex));
        }
    }

    public async Task<Either<ControlBoardError, Unit>> EmergencyStop()
    {
        if (_disposed)
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.ConnectionError("Control board has been disposed"));

        try
        {
            // 立即停止所有电机
            foreach (var motorId in _motorStates.Keys)
            {
                var currentState = _motorStates[motorId];
                _motorStates[motorId] = currentState with 
                { 
                    IsMoving = false, 
                    CurrentSpeed = 0f 
                };
            }

            // 将所有执行器设置为非活动状态
            foreach (var actuatorId in _actuatorStates.Keys)
            {
                var currentState = _actuatorStates[actuatorId];
                _actuatorStates[actuatorId] = currentState with 
                { 
                    CurrentState = "Stopped", 
                    IsActive = false 
                };
            }

            PublishState();
            await Task.CompletedTask;

            return Right<ControlBoardError, Unit>(unit);
        }
        catch (Exception ex)
        {
            return Left<ControlBoardError, Unit>(
                new ControlBoardError.CommandFailed("Emergency stop", ex.Message, ex));
        }
    }

    // 私有辅助方法

    private async Task<MotorState> SimulateMotorMoveTo(
        MotorId motorId,
        MotorState currentState,
        MotorAction.MoveTo moveTo)
    {
        // 模拟电机运动延迟
        var distance = Math.Abs(moveTo.Position - currentState.CurrentPosition);
        var travelTime = (int)(distance / moveTo.Speed * 1000); // 转换为毫秒
        var simulatedTime = Math.Min(travelTime, _config.MaxMotorTravelTime);

        // 设置为运动状态
        var movingState = currentState with 
        { 
            IsMoving = true, 
            CurrentSpeed = moveTo.Speed 
        };
        _motorStates[motorId] = movingState;
        PublishState();

        await Task.Delay(simulatedTime);

        // 到达目标位置
        return movingState with 
        { 
            CurrentPosition = moveTo.Position, 
            IsMoving = false, 
            CurrentSpeed = 0f 
        };
    }

    private async Task<MotorState> SimulateMotorRotateTo(
        MotorId motorId,
        MotorState currentState,
        MotorAction.RotateTo rotateTo)
    {
        // 模拟旋转运动延迟
        var angle = Math.Abs(rotateTo.Angle - currentState.CurrentPosition);
        var travelTime = (int)(angle / rotateTo.Speed * 1000);
        var simulatedTime = Math.Min(travelTime, _config.MaxMotorTravelTime);

        var movingState = currentState with 
        { 
            IsMoving = true, 
            CurrentSpeed = rotateTo.Speed 
        };
        _motorStates[motorId] = movingState;
        PublishState();

        await Task.Delay(simulatedTime);

        return movingState with 
        { 
            CurrentPosition = rotateTo.Angle, 
            IsMoving = false, 
            CurrentSpeed = 0f 
        };
    }

    private async Task<MotorState> SimulateMotorHome(
        MotorId motorId,
        MotorState currentState)
    {
        // 模拟回零延迟
        var movingState = currentState with 
        { 
            IsMoving = true, 
            CurrentSpeed = _config.DefaultHomeSpeed 
        };
        _motorStates[motorId] = movingState;
        PublishState();

        await Task.Delay(_config.HomeDelay);

        return movingState with 
        { 
            CurrentPosition = 0f, 
            IsMoving = false, 
            CurrentSpeed = 0f, 
            IsHomed = true 
        };
    }

    private async Task<ActuatorState> SimulateActuatorAction(
        ActuatorId actuatorId,
        string stateName,
        bool isActive)
    {
        // 模拟执行器动作延迟
        await Task.Delay(_config.ActuatorActionDelay);

        return new ActuatorState(stateName, isActive);
    }

    private SensorReading GenerateRandomSensorReading()
    {
        var sensorType = _random.Next(3);
        return sensorType switch
        {
            0 => new SensorReading.Pressure(_random.Next(0, 1000) / 10f, "kPa"),
            1 => new SensorReading.Micrometer(_random.Next(0, 10000) / 1000f),
            2 => new SensorReading.Scanner($"BARCODE_{_random.Next(10000, 99999)}"),
            _ => new SensorReading.Pressure(0f, "kPa")
        };
    }

    private ControlBoardState GetCurrentState()
    {
        return new ControlBoardState(
            DateTime.UtcNow,
            _isInitialized,
            toHashMap(_motorStates),
            toHashMap(_actuatorStates),
            toHashMap(_sensorReadings),
            toHashMap(_stateSensorStates));
    }

    private void PublishState()
    {
        if (!_disposed)
        {
            _stateSubject.OnNext(GetCurrentState());
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _stateSubject.OnCompleted();
            _stateSubject.Dispose();
        }
    }
}

/// <summary>
/// 模拟控制板配置
/// </summary>
public sealed record SimulatedControlBoardConfig(
    int InitializationDelay,
    int CommandDelay,
    int SensorReadDelay,
    int ActuatorActionDelay,
    int HomeDelay,
    int MaxMotorTravelTime,
    float DefaultHomeSpeed)
{
    public static readonly SimulatedControlBoardConfig Default = new(
        InitializationDelay: 100,
        CommandDelay: 10,
        SensorReadDelay: 5,
        ActuatorActionDelay: 50,
        HomeDelay: 200,
        MaxMotorTravelTime: 2000,
        DefaultHomeSpeed: 50f);
}
