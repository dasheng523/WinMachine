using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;
using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.ControlBoards.Abstractions;
using MachineOrchestration.ControlBoards.Implementations;
using MachineOrchestration.ControlBoards.Sdk;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Tests.Integration;

public class LeiSaiBoardTests : IDisposable
{
    private readonly MockLeiSaiSdk _mockSdk;
    private readonly LeiSaiBoard _board;
    private readonly LeiSaiBoardConfig _config;

    public LeiSaiBoardTests()
    {
        _mockSdk = new MockLeiSaiSdk(simulateErrors: false);
        _config = new LeiSaiBoardConfig(
            IpAddress: "127.0.0.1",
            Port: 8080,
            MaxRetries: 3,
            InitialRetryDelay: 10,
            StateUpdateInterval: 20,
            DefaultHomeSpeed: 50f);
        _board = new LeiSaiBoard(_mockSdk, _config);
    }

    [Fact]
    public async Task Initialize_ShouldConnectSuccessfully()
    {
        // Act
        var result = await _board.Initialize();

        // Assert
        Assert.True(result.IsRight);
        Assert.True(_mockSdk.IsConnected);
    }

    [Fact]
    public async Task Initialize_WhenConnectionFails_ShouldReturnError()
    {
        // Arrange
        var failingSdk = new MockLeiSaiSdk(simulateErrors: true, errorProbability: 1.0);
        var failingBoard = new LeiSaiBoard(failingSdk, _config);

        // Act
        var result = await failingBoard.Initialize();

        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ControlBoardError.ConnectionError>(error);
        });

        failingBoard.Dispose();
    }

    [Fact]
    public async Task SendMotorCommand_MoveTo_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        var moveTo = new MotorAction.MoveTo(100f, 50f);

        // Act
        var result = await _board.SendMotorCommand(motorId, moveTo);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendMotorCommand_RotateTo_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        var rotateTo = new MotorAction.RotateTo(90f, 30f);

        // Act
        var result = await _board.SendMotorCommand(motorId, rotateTo);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendMotorCommand_Home_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();

        // Act
        var result = await _board.SendMotorCommand(motorId, MotorAction.Home.Instance);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendMotorCommand_Stop_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();

        // Act
        var result = await _board.SendMotorCommand(motorId, MotorAction.Stop.Instance);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendMotorCommand_WhenNotInitialized_ShouldReturnError()
    {
        // Arrange
        var motorId = MotorId.NewId();
        var moveTo = new MotorAction.MoveTo(100f, 50f);

        // Act
        var result = await _board.SendMotorCommand(motorId, moveTo);

        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ControlBoardError.NotInitialized>(error);
        });
    }

    [Fact]
    public async Task SendActuatorCommand_Extend_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Extend.Instance);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendActuatorCommand_Retract_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Retract.Instance);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendActuatorCommand_Close_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Close.Instance);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SendActuatorCommand_Open_ShouldExecuteSuccessfully()
    {
        // Arrange
        await _board.Initialize();
        var actuatorId = ActuatorId.NewId();

        // Act
        var result = await _board.SendActuatorCommand(actuatorId, ActuatorAction.Open.Instance);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task ReadSensor_ShouldReturnSensorReading()
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
        });
    }

    [Fact]
    public async Task ReadStateSensor_ShouldReturnBooleanState()
    {
        // Arrange
        await _board.Initialize();
        var stateSensorId = StateSensorId.NewId();

        // Act
        var result = await _board.ReadStateSensor(stateSensorId);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task EmergencyStop_ShouldStopAllMotorsAndActuators()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        var actuatorId = ActuatorId.NewId();
        
        // 启动一些动作
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(100f, 50f));
        await _board.SendActuatorCommand(actuatorId, ActuatorAction.Extend.Instance);

        // Act
        var result = await _board.EmergencyStop();

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task StateStream_ShouldPublishStateUpdates()
    {
        // Arrange
        await _board.Initialize();
        var stateUpdates = new System.Collections.Generic.List<ControlBoardState>();
        var subscription = _board.StateStream
            .Take(3)
            .Subscribe(state => stateUpdates.Add(state));

        var motorId = MotorId.NewId();

        // Act
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(100f, 50f));
        await Task.Delay(100); // 等待状态更新

        // Assert
        Assert.NotEmpty(stateUpdates);
        Assert.True(stateUpdates.Any(s => s.IsInitialized));

        subscription.Dispose();
    }

    [Fact]
    public async Task ErrorHandling_WithTransientErrors_ShouldRetryAndSucceed()
    {
        // Arrange
        var unreliableSdk = new MockLeiSaiSdk(simulateErrors: true, errorProbability: 0.3);
        var unreliableBoard = new LeiSaiBoard(unreliableSdk, _config);
        await unreliableBoard.Initialize();
        
        var motorId = MotorId.NewId();
        var successCount = 0;
        var attempts = 10;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = await unreliableBoard.SendMotorCommand(
                motorId, 
                new MotorAction.MoveTo(100f, 50f));
            
            if (result.IsRight)
            {
                successCount++;
            }
        }

        // Assert
        // 由于重试机制，应该有一些成功的命令
        Assert.True(successCount > 0, "Expected some commands to succeed with retry logic");

        unreliableBoard.Dispose();
    }

    // Note: Retry logic test is covered by ErrorHandling_WithTransientErrors_ShouldRetryAndSucceed

    [Fact]
    public async Task MultipleMotors_ShouldMaintainSeparateStates()
    {
        // Arrange
        await _board.Initialize();
        var motor1 = MotorId.NewId();
        var motor2 = MotorId.NewId();

        // Act
        await _board.SendMotorCommand(motor1, new MotorAction.MoveTo(100f, 50f));
        await _board.SendMotorCommand(motor2, new MotorAction.MoveTo(200f, 30f));
        await Task.Delay(50); // 等待状态更新

        // Assert
        // 两个电机应该有独立的状态
        var states = await _board.StateStream.Take(1).FirstAsync();
        Assert.Contains(motor1, states.MotorStates.Keys);
        Assert.Contains(motor2, states.MotorStates.Keys);
    }

    [Fact]
    public async Task Dispose_ShouldCleanupResources()
    {
        // Arrange
        await _board.Initialize();
        var motorId = MotorId.NewId();
        await _board.SendMotorCommand(motorId, new MotorAction.MoveTo(100f, 50f));

        // Act
        _board.Dispose();

        // Assert
        var result = await _board.SendMotorCommand(motorId, MotorAction.Stop.Instance);
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            // After disposal, commands should fail (either NotInitialized or CommandFailed)
            Assert.True(
                error is ControlBoardError.NotInitialized or ControlBoardError.CommandFailed,
                $"Expected NotInitialized or CommandFailed, but got {error.GetType().Name}");
        });
    }

    public void Dispose()
    {
        _board?.Dispose();
    }
}
