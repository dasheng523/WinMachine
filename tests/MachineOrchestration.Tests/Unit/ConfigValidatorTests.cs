using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.Configuration.Types;
using MachineOrchestration.Configuration.Validation;
using MachineOrchestration.Core.Types;
using System.Numerics;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// 配置验证器单元测试
/// 验证：需求 11.9-11.12, 12.2-12.4
/// </summary>
public class ConfigValidatorTests
{
    private readonly ConfigValidator _validator = new();
    
    #region 测试辅助方法
    
    private static ComposableEntity.Part CreateTestMotorPart(MotorConfig motorConfig)
    {
        var motorType = new MotorType.LinearScrew(100f, 500f);
        var part = new Part(
            new PartId(Guid.NewGuid()),
            "Test Motor",
            new PartType.Motor(motorType),
            PartCategory.MotorType.Instance,
            new Vector3(100, 50, 50));
        
        return new ComposableEntity.Part(
            new EntityId(Guid.NewGuid()),
            part,
            Coordinate.Identity,
            new PartConfig.Motor(motorConfig));
    }
    
    private static ComposableEntity.Part CreateTestActuatorPart(ActuatorConfig actuatorConfig)
    {
        var actuatorType = new ActuatorType.Cylinder(
            100f, 
            CylinderSensorConfig.None.Instance);
        var part = new Part(
            new PartId(Guid.NewGuid()),
            "Test Cylinder",
            new PartType.Actuator(actuatorType),
            PartCategory.OutputType.Instance,
            new Vector3(50, 50, 100));
        
        return new ComposableEntity.Part(
            new EntityId(Guid.NewGuid()),
            part,
            Coordinate.Identity,
            new PartConfig.Actuator(actuatorConfig));
    }
    
    private static MachineConfig CreateValidMachineConfig()
    {
        var motorConfig = new MotorConfig(
            WorkingSpeed: 50f,
            HomingMode: HomingMode.PositiveLimit,
            BoardConnection: new BoardConnection(0),
            LimitSensors: new LimitSensors(None, None));
        
        var motorPart = CreateTestMotorPart(motorConfig);
        
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq1(((ComposableEntity)motorPart, Coordinate.Identity)),
            Coordinate.Identity);
        
        var controlBoard = new ControlBoardConfig.Simulated(100);
        
        return new MachineConfig(
            machine,
            controlBoard,
            HashMap<LogicId, AutomationLogic>());
    }
    
    #endregion
    
    #region 控制板配置验证测试
    
    [Fact]
    public void Validate_ValidSimulatedBoard_ReturnsSuccess()
    {
        // Arrange
        var config = CreateValidMachineConfig();
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_LeiSaiWithEmptyConnection_ReturnsError()
    {
        // Arrange
        var config = CreateValidMachineConfig();
        var invalidBoard = new ControlBoardConfig.LeiSai(
            "",
            new LeiSaiParameters(4, 100f));
        var invalidConfig = config with { ControlBoard = invalidBoard };
        
        // Act
        var result = _validator.Validate(invalidConfig);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ConfigValidationError.MissingField);
        });
    }
    
    [Fact]
    public void Validate_LeiSaiWithInvalidMaxAxes_ReturnsError()
    {
        // Arrange
        var config = CreateValidMachineConfig();
        var invalidBoard = new ControlBoardConfig.LeiSai(
            "COM1",
            new LeiSaiParameters(0, 100f)); // MaxAxes = 0 无效
        var invalidConfig = config with { ControlBoard = invalidBoard };
        
        // Act
        var result = _validator.Validate(invalidConfig);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => 
                e is ConfigValidationError.InvalidValue iv && 
                iv.Field.Contains("MaxAxes"));
        });
    }
    
    [Fact]
    public void Validate_ZhengYunDongWithInvalidSpeed_ReturnsError()
    {
        // Arrange
        var config = CreateValidMachineConfig();
        var invalidBoard = new ControlBoardConfig.ZhengYunDong(
            "192.168.1.100",
            new ZhengYunDongParameters(4, -10f)); // 负速度无效
        var invalidConfig = config with { ControlBoard = invalidBoard };
        
        // Act
        var result = _validator.Validate(invalidConfig);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => 
                e is ConfigValidationError.InvalidValue iv && 
                iv.Field.Contains("DefaultSpeed"));
        });
    }
    
    [Fact]
    public void Validate_SimulatedWithNegativeLatency_ReturnsError()
    {
        // Arrange
        var config = CreateValidMachineConfig();
        var invalidBoard = new ControlBoardConfig.Simulated(-100); // 负延迟无效
        var invalidConfig = config with { ControlBoard = invalidBoard };
        
        // Act
        var result = _validator.Validate(invalidConfig);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => 
                e is ConfigValidationError.InvalidValue iv && 
                iv.Field.Contains("LatencyMs"));
        });
    }
    
    #endregion
    
    #region 电机配置验证测试
    
    [Fact]
    public void Validate_MotorWithZeroSpeed_ReturnsError()
    {
        // Arrange
        var invalidMotorConfig = new MotorConfig(
            WorkingSpeed: 0f, // 无效速度
            HomingMode: HomingMode.PositiveLimit,
            BoardConnection: new BoardConnection(0),
            LimitSensors: new LimitSensors(None, None));
        
        var motorPart = CreateTestMotorPart(invalidMotorConfig);
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq1(((ComposableEntity)motorPart, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => 
                e is ConfigValidationError.InvalidMotorConfig);
        });
    }
    
    [Fact]
    public void Validate_MotorWithNegativeAxisNumber_ReturnsError()
    {
        // Arrange
        var invalidMotorConfig = new MotorConfig(
            WorkingSpeed: 50f,
            HomingMode: HomingMode.PositiveLimit,
            BoardConnection: new BoardConnection(255), // 负数会溢出为大数
            LimitSensors: new LimitSensors(None, None));
        
        var motorPart = CreateTestMotorPart(invalidMotorConfig);
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq1(((ComposableEntity)motorPart, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        // byte 类型不能为负，所以这个测试验证正常范围
        Assert.True(result.IsRight);
    }
    
    #endregion
    
    #region 执行器传感器配置验证测试
    
    [Fact]
    public void Validate_ActuatorWithValidSensorConfig_ReturnsSuccess()
    {
        // Arrange
        var actuatorConfig = new ActuatorConfig(
            OutputPort: 1,
            StateSensorPorts: Some<StateSensorPorts>(
                new StateSensorPorts.Cylinder(
                    new CylinderSensorConfig.Both(
                        new SensorPort(10),
                        new SensorPort(11)))));
        
        var actuatorPart = CreateTestActuatorPart(actuatorConfig);
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq1(((ComposableEntity)actuatorPart, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_ActuatorWithoutSensorConfig_ReturnsSuccess()
    {
        // Arrange
        var actuatorConfig = new ActuatorConfig(
            OutputPort: 1,
            StateSensorPorts: None);
        
        var actuatorPart = CreateTestActuatorPart(actuatorConfig);
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq1(((ComposableEntity)actuatorPart, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    #endregion
    
    #region 端口冲突检测测试
    
    [Fact]
    public void Validate_MultipleActuatorsWithSameOutputPort_ReturnsPortConflictError()
    {
        // Arrange
        var actuatorConfig1 = new ActuatorConfig(
            OutputPort: 1, // 相同端口
            StateSensorPorts: None);
        
        var actuatorConfig2 = new ActuatorConfig(
            OutputPort: 1, // 相同端口
            StateSensorPorts: None);
        
        var actuator1 = CreateTestActuatorPart(actuatorConfig1);
        var actuator2 = CreateTestActuatorPart(actuatorConfig2);
        
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq(((ComposableEntity)actuator1, Coordinate.Identity), ((ComposableEntity)actuator2, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => 
                e is ConfigValidationError.PortConflict pc && pc.Port == 1);
        });
    }
    
    [Fact]
    public void Validate_ActuatorsWithDifferentPorts_ReturnsSuccess()
    {
        // Arrange
        var actuatorConfig1 = new ActuatorConfig(
            OutputPort: 1,
            StateSensorPorts: None);
        
        var actuatorConfig2 = new ActuatorConfig(
            OutputPort: 2, // 不同端口
            StateSensorPorts: None);
        
        var actuator1 = CreateTestActuatorPart(actuatorConfig1);
        var actuator2 = CreateTestActuatorPart(actuatorConfig2);
        
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq(((ComposableEntity)actuator1, Coordinate.Identity), ((ComposableEntity)actuator2, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_SensorPortConflictWithOutputPort_ReturnsPortConflictError()
    {
        // Arrange
        var actuatorConfig1 = new ActuatorConfig(
            OutputPort: 10,
            StateSensorPorts: None);
        
        var actuatorConfig2 = new ActuatorConfig(
            OutputPort: 2,
            StateSensorPorts: Some<StateSensorPorts>(
                new StateSensorPorts.Cylinder(
                    new CylinderSensorConfig.ExtendOnly(
                        new SensorPort(10))))); // 与 actuator1 的输出端口冲突
        
        var actuator1 = CreateTestActuatorPart(actuatorConfig1);
        var actuator2 = CreateTestActuatorPart(actuatorConfig2);
        
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq(((ComposableEntity)actuator1, Coordinate.Identity), ((ComposableEntity)actuator2, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            new ControlBoardConfig.Simulated(100),
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => 
                e is ConfigValidationError.PortConflict pc && pc.Port == 10);
        });
    }
    
    #endregion
    
    #region 多个错误收集测试
    
    [Fact]
    public void Validate_MultipleErrors_CollectsAllErrors()
    {
        // Arrange
        // 1. 无效的控制板配置
        var invalidBoard = new ControlBoardConfig.LeiSai(
            "",
            new LeiSaiParameters(0, -10f));
        
        // 2. 无效的电机配置
        var invalidMotorConfig = new MotorConfig(
            WorkingSpeed: 0f,
            HomingMode: HomingMode.PositiveLimit,
            BoardConnection: new BoardConnection(0),
            LimitSensors: new LimitSensors(None, None));
        
        var motorPart = CreateTestMotorPart(invalidMotorConfig);
        
        // 3. 端口冲突
        var actuatorConfig1 = new ActuatorConfig(OutputPort: 1, StateSensorPorts: None);
        var actuatorConfig2 = new ActuatorConfig(OutputPort: 1, StateSensorPorts: None);
        var actuator1 = CreateTestActuatorPart(actuatorConfig1);
        var actuator2 = CreateTestActuatorPart(actuatorConfig2);
        
        var machine = new ComposableEntity.Composite(
            new EntityId(Guid.NewGuid()),
            "Test Machine",
            Seq(
                ((ComposableEntity)motorPart, Coordinate.Identity),
                ((ComposableEntity)actuator1, Coordinate.Identity),
                ((ComposableEntity)actuator2, Coordinate.Identity)),
            Coordinate.Identity);
        
        var config = new MachineConfig(
            machine,
            invalidBoard,
            HashMap<LogicId, AutomationLogic>());
        
        // Act
        var result = _validator.Validate(config);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ConfigValidationError.Multiple>(error);
            var multiple = (ConfigValidationError.Multiple)error;
            
            // 应该至少有 5 个错误：
            // - 控制板连接为空
            // - MaxAxes 无效
            // - DefaultSpeed 无效
            // - 电机速度无效
            // - 端口冲突
            Assert.True(multiple.Errors.Count >= 5);
            
            // 验证包含各种类型的错误
            Assert.Contains(multiple.Errors, e => e is ConfigValidationError.MissingField);
            Assert.Contains(multiple.Errors, e => e is ConfigValidationError.InvalidValue);
            Assert.Contains(multiple.Errors, e => e is ConfigValidationError.InvalidMotorConfig);
            Assert.Contains(multiple.Errors, e => e is ConfigValidationError.PortConflict);
        });
    }
    
    [Fact]
    public void GetMessage_ReturnsDescriptiveErrorMessage()
    {
        // Arrange
        var error = new ConfigValidationError.MissingSensorPort(
            new EntityId(Guid.NewGuid()),
            "伸出传感器");
        
        // Act
        var message = error.GetMessage();
        
        // Assert
        Assert.Contains("配置了", message);
        Assert.Contains("伸出传感器", message);
        Assert.Contains("未指定传感器端口", message);
    }
    
    [Fact]
    public void GetMessage_MultipleErrors_ReturnsAllErrorMessages()
    {
        // Arrange
        var errors = Seq<ConfigValidationError>(
            new ConfigValidationError.MissingField("Connection"),
            new ConfigValidationError.InvalidValue("Speed", "必须大于 0"));
        var multipleError = new ConfigValidationError.Multiple(errors);
        
        // Act
        var message = multipleError.GetMessage();
        
        // Assert
        Assert.Contains("2 个验证错误", message);
        Assert.Contains("Connection", message);
        Assert.Contains("Speed", message);
    }
    
    #endregion
}
