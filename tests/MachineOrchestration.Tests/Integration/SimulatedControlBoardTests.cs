using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MachineOrchestration.ControlBoards.Implementations;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;
using Xunit;

namespace MachineOrchestration.Tests.Integration;

/// <summary>
/// 模拟控制板集成测试
/// 验证模拟控制板的完整功能
/// </summary>
public class SimulatedControlBoardTests : IDisposable
{
    private readonly SimulatedControlBoard _board;
    private readonly SimulatedControlBoardConfig _config;

    public SimulatedControlBoardTests()
    {
        // 使用更快的配置以加速测试
        _config = new SimulatedControlBoardConfig(
            InitializationDelay: 10,
            CommandDelay: 5,
            SensorReadDelay: 5,
            ActuatorActionDelay: 10,
            HomeDelay: 20,
            MaxMotorTravelTime: 100,
            DefaultHomeSpeed: 100f);
        
        _board = new SimulatedControlBoard(_config);
    }

    [Fact]
    public async Task Initialize_ShouldSucceed()
    {
        // Act
        var result = await _board.Initialize();

        // Assert
        Assert.True(result.IsRight);
        result.IfRight(unit => Assert.Equal(LanguageExt.Unit.Default, unit));
    }

    [Fact]
    public async Task Initialize_ShouldPublishStateUpdate()
    {
        // Arrange
        ControlBoardState? capturedState = null;
        using var subscription = _board.StateStream.Subscribe(state => capturedState = state);

        // Act
        await _board.Initialize();
        await Task.Delay(50); // 等待状态发布

        // Assert
        Assert.NotNull(capturedState);
        Assert.True(capturedState!.IsInitialized);
    }

    [Fact]
    public async Task SendMotorCommand_WithoutInitialization_ShouldReturnNotInitializedError()
    {
        // Arrange
        var motorId = MotorId.NewId();
        var action = MotorAction.Home.Instance;

        // Act
        var result = await _board.SendMotorCommand(motorId, action);

        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error => Assert.IsType<ControlBoardError.NotInitialized>(error));
    }

    [Fact]
    public async Task SendMotorCommand_MoveTo_ShouldUpdateMotorState()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        var targetPosition = 100f;
        var speed = 50f;
        var action = new MotorAction.MoveTo(targetPosition, speed);

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Skip(1) // 跳过初始状态
            .Subscribe(state => finalState = state);

        // Act
        var result = await _board.SendMotorCommand(motorId, action);
        await Task.Delay(200); // 等待运动完成

        // Assert
        Assert.True(result.IsRight);
        Assert.NotNull(finalState);
        Assert.True(finalState!.MotorStates.ContainsKey(motorId));
        
        var motorState = finalState.MotorStates[motorId];
        Assert.Equal(targetPosition, motorState.CurrentPosition);
        Assert.False(motorState.IsMoving);
        Assert.Equal(0f, motorState.CurrentSpeed);
    }

    [Fact]
    public async Task SendMotorCommand_RotateTo_ShouldUpdateMotorState()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        var targetAngle = 90f;
        var speed = 30f;
        var action = new MotorAction.RotateTo(targetAngle, speed);

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Skip(1)
            .Subscribe(state => finalState = state);

        // Act
        var result = await _board.SendMotorCommand(motorId, action);
        await Task.Delay(200);

        // Assert
        Assert.True(result.IsRight);
        Assert.NotNull(finalState);
        Assert.True(finalState!.MotorStates.ContainsKey(motorId));
        
        var motorState = finalState.MotorStates[motorId];
        Assert.Equal(targetAngle, motorState.CurrentPosition);
        Assert.False(motorState.IsMoving);
    }

    [Fact]
    public async Task SendMotorCommand_Home_ShouldResetPositionAndSetHomedFlag()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        
        // 先移动到某个位置
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(50f, 25f));
        await Task.Delay(150);

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Subscribe(state => finalState = state);

        // Act
        var result = await _board.SendMotorCommand(motorId, MotorAction.Home.Instance);
        await Task.Delay(100);

        // Assert
        Assert.True(result.IsRight);
        Assert.NotNull(finalState);
        
        var motorState = finalState!.MotorStates[motorId];
        Assert.Equal(0f, motorState.CurrentPosition);
        Assert.True(motorState.IsHomed);
        Assert.False(motorState.IsMoving);
    }

    [Fact]
    public async Task SendMotorCommand_Stop_ShouldStopMotor()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        
        // 开始移动
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(100f, 10f));
        await Task.Delay(20); // 让电机开始运动

        // Act
        var result = await _board.SendMotorCommand(motorId, MotorAction.Stop.Instance);
        await Task.Delay(50);

        // Assert
        Assert.True(result.IsRight);
        
        var state = await _board.StateStream.FirstAsync();
        var motorState = state.MotorStates[motorId];
        Assert.False(motorState.IsMoving);
        Assert.Equal(0f, motorState.CurrentSpeed);
    }

    [Fact]
    public async Task SendActuatorCommand_Extend_ShouldUpdateActuatorState()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Skip(1)
            .Subscribe(state => finalState = state);

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Extend.Instance);
        await Task.Delay(100);

        // Assert
        Assert.True(result.IsRight);
        Assert.NotNull(finalState);
        Assert.True(finalState!.ActuatorStates.ContainsKey(actuatorId));
        
        var actuatorState = finalState.ActuatorStates[actuatorId];
        Assert.Equal("Extended", actuatorState.CurrentState);
        Assert.True(actuatorState.IsActive);
    }

    [Fact]
    public async Task SendActuatorCommand_Retract_ShouldUpdateActuatorState()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Skip(1)
            .Subscribe(state => finalState = state);

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Retract.Instance);
        await Task.Delay(100);

        // Assert
        Assert.True(result.IsRight);
        Assert.NotNull(finalState);
        
        var actuatorState = finalState!.ActuatorStates[actuatorId];
        Assert.Equal("Retracted", actuatorState.CurrentState);
        Assert.False(actuatorState.IsActive);
    }

    [Fact]
    public async Task SendActuatorCommand_Close_ShouldUpdateActuatorState()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Close.Instance);
        await Task.Delay(100);

        // Assert
        Assert.True(result.IsRight);
        
        var state = await _board.StateStream.FirstAsync();
        var actuatorState = state.ActuatorStates[actuatorId];
        Assert.Equal("Closed", actuatorState.CurrentState);
        Assert.True(actuatorState.IsActive);
    }

    [Fact]
    public async Task SendActuatorCommand_Open_ShouldUpdateActuatorState()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Open.Instance);
        await Task.Delay(100);

        // Assert
        Assert.True(result.IsRight);
        
        var state = await _board.StateStream.FirstAsync();
        var actuatorState = state.ActuatorStates[actuatorId];
        Assert.Equal("Opened", actuatorState.CurrentState);
        Assert.False(actuatorState.IsActive);
    }

    [Fact]
    public async Task ReadSensor_ShouldReturnRandomReading()
    {
        // Arrange
        await _board.Initialize();
        var sensorId = SensorId.NewId();

        // Act
        var result = await _board.ReadSensor(sensorId);

        // Assert
        Assert.True(result.IsRight);
        result.IfRight(reading =>
        {
            Assert.NotNull(reading);
            Assert.True(
                reading is SensorReading.Pressure ||
                reading is SensorReading.Micrometer ||
                reading is SensorReading.Scanner);
        });
    }

    [Fact]
    public async Task ReadSensor_ShouldUpdateStateStream()
    {
        // Arrange
        await _board.Initialize();
        var sensorId = SensorId.NewId();

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Skip(1)
            .Subscribe(state => finalState = state);

        // Act
        await _board.ReadSensor(sensorId);
        await Task.Delay(50);

        // Assert
        Assert.NotNull(finalState);
        Assert.True(finalState!.SensorReadings.ContainsKey(sensorId));
        Assert.True(finalState.SensorReadings[sensorId].IsSome);
    }

    [Fact]
    public async Task ReadStateSensor_ShouldReturnBooleanValue()
    {
        // Arrange
        await _board.Initialize();
        var stateSensorId = StateSensorId.NewId();

        // Act
        var result = await _board.ReadStateSensor(stateSensorId);

        // Assert
        Assert.True(result.IsRight);
        result.IfRight(state => Assert.True(state == true || state == false));
    }

    [Fact]
    public async Task ReadStateSensor_ShouldUpdateStateStream()
    {
        // Arrange
        await _board.Initialize();
        var stateSensorId = StateSensorId.NewId();

        ControlBoardState? finalState = null;
        using var subscription = _board.StateStream
            .Skip(1)
            .Subscribe(state => finalState = state);

        // Act
        await _board.ReadStateSensor(stateSensorId);
        await Task.Delay(50);

        // Assert
        Assert.NotNull(finalState);
        Assert.True(finalState!.StateSensorStates.ContainsKey(stateSensorId));
    }

    [Fact]
    public async Task EmergencyStop_ShouldStopAllMotorsAndActuators()
    {
        // Arrange
        await _board.Initialize();
        
        var motorId1 = MotorId.NewId();
        var motorId2 = MotorId.NewId();
        var actuatorId = ActuatorId.NewId();

        // 启动一些运动
        await _board.SendMotorCommand(motorId1, new MotorAction.MoveTo(100f, 10f));
        await _board.SendMotorCommand(motorId2, new MotorAction.RotateTo(180f, 20f));
        await _board.SendActuatorCommand(actuatorId, ActuatorAction.Extend.Instance);
        await Task.Delay(20);

        // Act
        var result = await _board.EmergencyStop();
        await Task.Delay(50);

        // Assert
        Assert.True(result.IsRight);
        
        var state = await _board.StateStream.FirstAsync();
        
        // 验证所有电机已停止
        foreach (var motorState in state.MotorStates.Values)
        {
            Assert.False(motorState.IsMoving);
            Assert.Equal(0f, motorState.CurrentSpeed);
        }
        
        // 验证所有执行器已停止
        foreach (var actuatorState in state.ActuatorStates.Values)
        {
            Assert.Equal("Stopped", actuatorState.CurrentState);
            Assert.False(actuatorState.IsActive);
        }
    }

    [Fact]
    public async Task StateStream_ShouldEmitUpdatesOnStateChanges()
    {
        // Arrange
        await _board.Initialize();
        var stateUpdates = new System.Collections.Generic.List<ControlBoardState>();
        
        using var subscription = _board.StateStream
            .Take(5) // 收集前5个状态更新
            .Subscribe(state => stateUpdates.Add(state));

        // Act
        var motorId = MotorId.NewId();
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(50f, 25f));
        await _board.SendMotorCommand(motorId, MotorAction.Stop.Instance);
        await Task.Delay(200);

        // Assert
        Assert.True(stateUpdates.Count >= 3); // 至少：初始化、移动开始、停止
    }

    [Fact]
    public async Task MultipleCommands_ShouldMaintainConsistentState()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        var actuatorId = ActuatorId.NewId();

        // Act - 执行一系列命令
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(30f, 15f));
        await Task.Delay(150);
        await _board.SendActuatorCommand(actuatorId, ActuatorAction.Close.Instance);
        await Task.Delay(50);
        await _board.SendMotorCommand(motorId, MotorAction.Home.Instance);
        await Task.Delay(100);

        // Assert
        var state = await _board.StateStream.FirstAsync();
        
        Assert.True(state.MotorStates.ContainsKey(motorId));
        Assert.True(state.ActuatorStates.ContainsKey(actuatorId));
        
        var motorState = state.MotorStates[motorId];
        Assert.Equal(0f, motorState.CurrentPosition);
        Assert.True(motorState.IsHomed);
        
        var actuatorState = state.ActuatorStates[actuatorId];
        Assert.Equal("Closed", actuatorState.CurrentState);
    }

    public void Dispose()
    {
        _board.Dispose();
    }
}
