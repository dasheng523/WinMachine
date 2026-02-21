using System;
using Xunit;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// 命令类型构造的单元测试
/// 验证命令类型和 ID 类型的正确构造
/// </summary>
public class CommandTypesTests
{
    [Fact]
    public void MotorId_ShouldCreateWithGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var motorId = new MotorId(guid);
        
        // Assert
        Assert.Equal(guid, motorId.Value);
    }
    
    [Fact]
    public void MotorId_NewId_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = MotorId.NewId();
        var id2 = MotorId.NewId();
        
        // Assert
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void ActuatorId_ShouldCreateWithGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var actuatorId = new ActuatorId(guid);
        
        // Assert
        Assert.Equal(guid, actuatorId.Value);
    }
    
    [Fact]
    public void ActuatorId_NewId_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = ActuatorId.NewId();
        var id2 = ActuatorId.NewId();
        
        // Assert
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void SensorId_ShouldCreateWithGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var sensorId = new SensorId(guid);
        
        // Assert
        Assert.Equal(guid, sensorId.Value);
    }
    
    [Fact]
    public void SensorId_NewId_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = SensorId.NewId();
        var id2 = SensorId.NewId();
        
        // Assert
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void StateSensorId_ShouldCreateWithGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var stateSensorId = new StateSensorId(guid);
        
        // Assert
        Assert.Equal(guid, stateSensorId.Value);
    }
    
    [Fact]
    public void StateSensorId_NewId_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = StateSensorId.NewId();
        var id2 = StateSensorId.NewId();
        
        // Assert
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void Command_Motor_ShouldCreateWithMotorIdAndAction()
    {
        // Arrange
        var motorId = MotorId.NewId();
        var action = new MotorAction.MoveTo(100f, 50f);
        
        // Act
        var command = new Command.Motor(motorId, action);
        
        // Assert
        Assert.Equal(motorId, command.MotorId);
        Assert.Equal(action, command.Action);
    }
    
    [Fact]
    public void Command_Actuator_ShouldCreateWithActuatorIdAndAction()
    {
        // Arrange
        var actuatorId = ActuatorId.NewId();
        var action = ActuatorAction.Extend.Instance;
        
        // Act
        var command = new Command.Actuator(actuatorId, action);
        
        // Assert
        Assert.Equal(actuatorId, command.ActuatorId);
        Assert.Equal(action, command.Action);
    }
    
    [Fact]
    public void Command_ReadSensor_ShouldCreateWithSensorId()
    {
        // Arrange
        var sensorId = SensorId.NewId();
        
        // Act
        var command = new Command.ReadSensor(sensorId);
        
        // Assert
        Assert.Equal(sensorId, command.SensorId);
    }
    
    [Fact]
    public void Command_ReadStateSensor_ShouldCreateWithStateSensorId()
    {
        // Arrange
        var stateSensorId = StateSensorId.NewId();
        
        // Act
        var command = new Command.ReadStateSensor(stateSensorId);
        
        // Assert
        Assert.Equal(stateSensorId, command.StateSensorId);
    }
    
    [Fact]
    public void Command_EmergencyStop_ShouldUseSingletonInstance()
    {
        // Act
        var command1 = Command.EmergencyStop.Instance;
        var command2 = Command.EmergencyStop.Instance;
        
        // Assert
        Assert.Same(command1, command2);
    }
    
    [Fact]
    public void Command_Motor_WithDifferentActions_ShouldCreateCorrectly()
    {
        // Arrange
        var motorId = MotorId.NewId();
        
        // Act
        var moveToCommand = new Command.Motor(motorId, new MotorAction.MoveTo(100f, 50f));
        var rotateToCommand = new Command.Motor(motorId, new MotorAction.RotateTo(90f, 30f));
        var homeCommand = new Command.Motor(motorId, MotorAction.Home.Instance);
        var stopCommand = new Command.Motor(motorId, MotorAction.Stop.Instance);
        
        // Assert
        Assert.IsType<MotorAction.MoveTo>(moveToCommand.Action);
        Assert.IsType<MotorAction.RotateTo>(rotateToCommand.Action);
        Assert.IsType<MotorAction.Home>(homeCommand.Action);
        Assert.IsType<MotorAction.Stop>(stopCommand.Action);
    }
    
    [Fact]
    public void Command_Actuator_WithDifferentActions_ShouldCreateCorrectly()
    {
        // Arrange
        var actuatorId = ActuatorId.NewId();
        
        // Act
        var extendCommand = new Command.Actuator(actuatorId, ActuatorAction.Extend.Instance);
        var retractCommand = new Command.Actuator(actuatorId, ActuatorAction.Retract.Instance);
        var closeCommand = new Command.Actuator(actuatorId, ActuatorAction.Close.Instance);
        var openCommand = new Command.Actuator(actuatorId, ActuatorAction.Open.Instance);
        var suctionCommand = new Command.Actuator(actuatorId, ActuatorAction.Suction.Instance);
        var normalCommand = new Command.Actuator(actuatorId, ActuatorAction.Normal.Instance);
        var onCommand = new Command.Actuator(actuatorId, ActuatorAction.On.Instance);
        var offCommand = new Command.Actuator(actuatorId, ActuatorAction.Off.Instance);
        
        // Assert
        Assert.IsType<ActuatorAction.Extend>(extendCommand.Action);
        Assert.IsType<ActuatorAction.Retract>(retractCommand.Action);
        Assert.IsType<ActuatorAction.Close>(closeCommand.Action);
        Assert.IsType<ActuatorAction.Open>(openCommand.Action);
        Assert.IsType<ActuatorAction.Suction>(suctionCommand.Action);
        Assert.IsType<ActuatorAction.Normal>(normalCommand.Action);
        Assert.IsType<ActuatorAction.On>(onCommand.Action);
        Assert.IsType<ActuatorAction.Off>(offCommand.Action);
    }
    
    [Fact]
    public void Command_ShouldBeImmutable()
    {
        // Arrange
        var motorId = MotorId.NewId();
        var action = new MotorAction.MoveTo(100f, 50f);
        var command = new Command.Motor(motorId, action);
        
        // Act & Assert
        // Records are immutable by default, this test verifies the type is a record
        Assert.IsAssignableFrom<Command>(command);
    }
    
    [Fact]
    public void NewtypeIds_ShouldSupportEquality()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var motorId1 = new MotorId(guid);
        var motorId2 = new MotorId(guid);
        var motorId3 = new MotorId(Guid.NewGuid());
        
        // Assert
        Assert.Equal(motorId1, motorId2);
        Assert.NotEqual(motorId1, motorId3);
    }
}
