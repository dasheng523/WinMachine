using System;
using System.Numerics;
using LanguageExt;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Dsl.Ast;
using MachineOrchestration.Dsl.Validation;
using Xunit;
using static LanguageExt.Prelude;
using Ast = MachineOrchestration.Dsl.Ast.Ast;

namespace MachineOrchestration.Tests.Unit;

/// <summary>DSL 验证器单元测试</summary>
/// <remarks>
/// 测试 DSL 语义验证器的各种场景，包括：
/// - 实体 ID 验证
/// - 传感器引用验证
/// - 动作兼容性验证
/// - 状态传感器引用验证
/// 验证：需求 8.3, 9.2, 28.8
/// </remarks>
public class DslValidatorTests
{
    private readonly IDslValidator _validator = new DslValidator();
    
    /// <summary>创建测试用的简单机器</summary>
    private static ComposableEntity CreateTestMachine()
    {
        // 创建一个电机零件
        var motorId = EntityId.NewId();
        var motorPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Motor",
            new PartType.Motor(new MotorType.LinearScrew(100f, 500f)),
            PartCategory.MotorType.Instance,
            new Vector3(100, 50, 50));
        
        var motorConfig = new PartConfig.Motor(
            new MotorConfig(
                50f,
                HomingMode.PositiveLimit,
                new BoardConnection(1),
                new LimitSensors(None, None)));
        
        var motorEntity = new ComposableEntity.Part(
            motorId,
            motorPart,
            Coordinate.Identity,
            motorConfig);
        
        // 创建一个气缸零件
        var cylinderId = EntityId.NewId();
        var cylinderPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Cylinder",
            new PartType.Actuator(new ActuatorType.Cylinder(
                100f,
                CylinderSensorConfig.None.Instance)),
            PartCategory.OutputType.Instance,
            new Vector3(50, 50, 100));
        
        var cylinderConfig = new PartConfig.Actuator(
            new ActuatorConfig(1, None));
        
        var cylinderEntity = new ComposableEntity.Part(
            cylinderId,
            cylinderPart,
            Coordinate.Identity,
            cylinderConfig);
        
        // 创建一个传感器零件
        var sensorId = EntityId.NewId();
        var sensorPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Sensor",
            new PartType.Sensor(new SensorType.Pressure(100f, PressureUnit.Bar)),
            PartCategory.InputType.Instance,
            new Vector3(20, 20, 20));
        
        var sensorConfig = new PartConfig.Sensor(
            new SensorConfig(new SensorConnection.SerialSingle("COM1", 9600)));
        
        var sensorEntity = new ComposableEntity.Part(
            sensorId,
            sensorPart,
            Coordinate.Identity,
            sensorConfig);
        
        // 组合成机器
        return new ComposableEntity.Composite(
            EntityId.NewId(),
            "Test Machine",
            Seq<(ComposableEntity, Coordinate)>(
                (motorEntity, Coordinate.Identity),
                (cylinderEntity, Coordinate.Identity),
                (sensorEntity, Coordinate.Identity)),
            Coordinate.Identity);
    }
    
    [Fact]
    public void Validate_EmptyAst_ShouldSucceed()
    {
        // Arrange
        var ast = Ast.Empty;
        var machine = CreateTestMachine();
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_ValidMotorAction_ShouldSucceed()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        
        var ast = Ast.Single(
            new Statement.Action(
                motorId,
                new PartAction.Motor(new MotorAction.MoveTo(100f, 50f))));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_ValidActuatorAction_ShouldSucceed()
    {
        // Arrange
        var machine = CreateTestMachine();
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        
        var ast = Ast.Single(
            new Statement.Action(
                cylinderId,
                new PartAction.Actuator(ActuatorAction.Extend.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_NonExistentEntity_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var nonExistentId = EntityId.NewId();
        
        var ast = Ast.Single(
            new Statement.Action(
                nonExistentId,
                new PartAction.Motor(MotorAction.Home.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.EntityNotFound);
        });
    }
    
    [Fact]
    public void Validate_IncompatibleAction_MotorActionOnActuator_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        
        var ast = Ast.Single(
            new Statement.Action(
                cylinderId,
                new PartAction.Motor(new MotorAction.MoveTo(100f, 50f))));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_IncompatibleAction_ActuatorActionOnMotor_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        
        var ast = Ast.Single(
            new Statement.Action(
                motorId,
                new PartAction.Actuator(ActuatorAction.Extend.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_ActionOnSensor_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        
        var ast = Ast.Single(
            new Statement.Action(
                sensorId,
                new PartAction.Motor(MotorAction.Home.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_ValidSensorCondition_ShouldSucceed()
    {
        // Arrange
        var machine = CreateTestMachine();
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.SensorState(sensorId, true)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_InvalidSensorReference_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        
        // 尝试将电机作为传感器引用
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.SensorState(motorId, true)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.InvalidSensorReference);
        });
    }
    
    [Fact]
    public void Validate_ComplexSequence_ShouldValidateAllStatements()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        
        var ast = Ast.Single(
            new Statement.Sequence(Seq<Statement>(
                new Statement.Action(
                    motorId,
                    new PartAction.Motor(MotorAction.Home.Instance)),
                new Statement.WaitUntil(
                    new Condition.SensorState(sensorId, true)),
                new Statement.Action(
                    cylinderId,
                    new PartAction.Actuator(ActuatorAction.Extend.Instance))
            )));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_WrongActuatorAction_CylinderWithGripperAction_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        
        // 尝试对气缸执行夹爪动作
        var ast = Ast.Single(
            new Statement.Action(
                cylinderId,
                new PartAction.Actuator(ActuatorAction.Close.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_MultipleErrors_ShouldCollectAll()
    {
        // Arrange
        var machine = CreateTestMachine();
        var nonExistentId1 = EntityId.NewId();
        var nonExistentId2 = EntityId.NewId();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        
        // 创建包含多个错误的 AST
        var ast = Ast.Single(
            new Statement.Sequence(Seq<Statement>(
                // 错误 1: 不存在的实体
                new Statement.Action(
                    nonExistentId1,
                    new PartAction.Motor(MotorAction.Home.Instance)),
                // 错误 2: 另一个不存在的实体
                new Statement.Action(
                    nonExistentId2,
                    new PartAction.Motor(new MotorAction.MoveTo(100f, 50f))),
                // 错误 3: 不兼容的动作（对电机执行执行器动作）
                new Statement.Action(
                    motorId,
                    new PartAction.Actuator(ActuatorAction.Extend.Instance))
            )));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            
            // 应该收集到 3 个错误
            Assert.Equal(3, multiple.Errors.Count);
            
            // 验证包含所有类型的错误
            Assert.Contains(multiple.Errors, e => e is ValidationError.EntityNotFound);
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_NonExistentSensorInCondition_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var nonExistentSensorId = EntityId.NewId();
        
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.SensorState(nonExistentSensorId, true)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.InvalidSensorReference);
        });
    }
    
    [Fact]
    public void Validate_ComplexCondition_AndOr_ShouldValidateAllReferences()
    {
        // Arrange
        var machine = CreateTestMachine();
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        
        // 创建复杂条件：(sensor AND NOT motor) OR sensor
        // motor 不是传感器，应该失败
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.Or(
                    new Condition.And(
                        new Condition.SensorState(sensorId, true),
                        new Condition.Not(new Condition.SensorState(motorId, false))),
                    new Condition.SensorState(sensorId, true))));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.InvalidSensorReference);
        });
    }
    
    [Fact]
    public void Validate_IfStatement_WithInvalidCondition_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        
        // If 语句的条件引用了非传感器实体
        var ast = Ast.Single(
            new Statement.If(
                new Condition.SensorState(motorId, true),
                new Statement.Action(
                    cylinderId,
                    new PartAction.Actuator(ActuatorAction.Extend.Instance)),
                None));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.InvalidSensorReference);
        });
    }
    
    [Fact]
    public void Validate_IfStatement_WithInvalidThenBranch_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        var nonExistentId = EntityId.NewId();
        
        // If 语句的 then 分支引用了不存在的实体
        var ast = Ast.Single(
            new Statement.If(
                new Condition.SensorState(sensorId, true),
                new Statement.Action(
                    nonExistentId,
                    new PartAction.Motor(MotorAction.Home.Instance)),
                None));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.EntityNotFound);
        });
    }
    
    [Fact]
    public void Validate_IfStatement_WithInvalidElseBranch_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        
        // If 语句的 else 分支包含不兼容的动作
        var ast = Ast.Single(
            new Statement.If(
                new Condition.SensorState(sensorId, true),
                new Statement.Action(
                    motorId,
                    new PartAction.Motor(MotorAction.Home.Instance)),
                Some<Statement>(new Statement.Action(
                    cylinderId,
                    new PartAction.Motor(new MotorAction.MoveTo(100f, 50f))))));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_LoopStatement_WithInvalidBody_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var nonExistentId = EntityId.NewId();
        
        // Loop 语句的 body 引用了不存在的实体
        var ast = Ast.Single(
            new Statement.Loop(
                Some<uint>(3),
                new Statement.Action(
                    nonExistentId,
                    new PartAction.Motor(MotorAction.Home.Instance))));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.EntityNotFound);
        });
    }
    
    [Fact]
    public void Validate_ParallelStatement_WithMultipleErrors_ShouldCollectAll()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        var cylinderId = GetFirstEntityOfType<PartType.Actuator>(machine);
        var nonExistentId = EntityId.NewId();
        
        // Parallel 语句包含多个错误
        var ast = Ast.Single(
            new Statement.Parallel(Seq<Statement>(
                // 错误 1: 不存在的实体
                new Statement.Action(
                    nonExistentId,
                    new PartAction.Motor(MotorAction.Home.Instance)),
                // 错误 2: 不兼容的动作
                new Statement.Action(
                    motorId,
                    new PartAction.Actuator(ActuatorAction.Extend.Instance)),
                // 错误 3: 另一个不兼容的动作
                new Statement.Action(
                    cylinderId,
                    new PartAction.Motor(new MotorAction.MoveTo(100f, 50f)))
            )));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            
            // 应该收集到 3 个错误
            Assert.Equal(3, multiple.Errors.Count);
        });
    }
    
    [Fact]
    public void Validate_SensorValueCondition_WithValidSensor_ShouldSucceed()
    {
        // Arrange
        var machine = CreateTestMachine();
        var sensorId = GetFirstEntityOfType<PartType.Sensor>(machine);
        
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.SensorValue(
                    sensorId,
                    ComparisonOp.Greater,
                    50.0f)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_SensorValueCondition_WithInvalidSensor_ShouldFail()
    {
        // Arrange
        var machine = CreateTestMachine();
        var motorId = GetFirstEntityOfType<PartType.Motor>(machine);
        
        // 尝试将电机作为传感器引用
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.SensorValue(
                    motorId,
                    ComparisonOp.Less,
                    100.0f)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.InvalidSensorReference);
        });
    }
    
    [Fact]
    public void Validate_StateSensorCondition_WithNoActuators_ShouldFail()
    {
        // Arrange
        // 创建一个只有电机和传感器的机器（没有执行器）
        var motorId = EntityId.NewId();
        var motorPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Motor",
            new PartType.Motor(new MotorType.LinearScrew(100f, 500f)),
            PartCategory.MotorType.Instance,
            new Vector3(100, 50, 50));
        
        var motorConfig = new PartConfig.Motor(
            new MotorConfig(
                50f,
                HomingMode.PositiveLimit,
                new BoardConnection(1),
                new LimitSensors(None, None)));
        
        var motorEntity = new ComposableEntity.Part(
            motorId,
            motorPart,
            Coordinate.Identity,
            motorConfig);
        
        var machine = new ComposableEntity.Composite(
            EntityId.NewId(),
            "Test Machine",
            Seq1(((ComposableEntity)motorEntity, Coordinate.Identity)),
            Coordinate.Identity);
        
        var stateSensorId = new StateSensorId(Guid.NewGuid());
        
        var ast = Ast.Single(
            new Statement.WaitUntil(
                new Condition.StateSensor(stateSensorId, true)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.InvalidStateSensorReference);
        });
    }
    
    [Fact]
    public void Validate_GripperWithWrongAction_ShouldFail()
    {
        // Arrange
        // 创建一个夹爪零件
        var gripperId = EntityId.NewId();
        var gripperPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Gripper",
            new PartType.Actuator(new ActuatorType.Gripper(
                50f,
                None)),
            PartCategory.OutputType.Instance,
            new Vector3(50, 50, 50));
        
        var gripperConfig = new PartConfig.Actuator(
            new ActuatorConfig(1, None));
        
        var gripperEntity = new ComposableEntity.Part(
            gripperId,
            gripperPart,
            Coordinate.Identity,
            gripperConfig);
        
        var machine = new ComposableEntity.Composite(
            EntityId.NewId(),
            "Test Machine",
            Seq1(((ComposableEntity)gripperEntity, Coordinate.Identity)),
            Coordinate.Identity);
        
        // 尝试对夹爪执行气缸动作
        var ast = Ast.Single(
            new Statement.Action(
                gripperId,
                new PartAction.Actuator(ActuatorAction.Extend.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<ValidationError.Multiple>(error);
            var multiple = (ValidationError.Multiple)error;
            Assert.Contains(multiple.Errors, e => e is ValidationError.IncompatibleAction);
        });
    }
    
    [Fact]
    public void Validate_SuctionWithCorrectAction_ShouldSucceed()
    {
        // Arrange
        // 创建一个吸气装置零件
        var suctionId = EntityId.NewId();
        var suctionPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Suction",
            new PartType.Actuator(new ActuatorType.Suction(None)),
            PartCategory.OutputType.Instance,
            new Vector3(30, 30, 30));
        
        var suctionConfig = new PartConfig.Actuator(
            new ActuatorConfig(1, None));
        
        var suctionEntity = new ComposableEntity.Part(
            suctionId,
            suctionPart,
            Coordinate.Identity,
            suctionConfig);
        
        var machine = new ComposableEntity.Composite(
            EntityId.NewId(),
            "Test Machine",
            Seq1(((ComposableEntity)suctionEntity, Coordinate.Identity)),
            Coordinate.Identity);
        
        var ast = Ast.Single(
            new Statement.Action(
                suctionId,
                new PartAction.Actuator(ActuatorAction.Suction.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    [Fact]
    public void Validate_IndicatorWithCorrectAction_ShouldSucceed()
    {
        // Arrange
        // 创建一个指示灯零件
        var indicatorId = EntityId.NewId();
        var indicatorPart = new Part(
            new PartId(Guid.NewGuid()),
            "Test Indicator",
            new PartType.Actuator(ActuatorType.Indicator.Instance),
            PartCategory.OutputType.Instance,
            new Vector3(10, 10, 10));
        
        var indicatorConfig = new PartConfig.Actuator(
            new ActuatorConfig(1, None));
        
        var indicatorEntity = new ComposableEntity.Part(
            indicatorId,
            indicatorPart,
            Coordinate.Identity,
            indicatorConfig);
        
        var machine = new ComposableEntity.Composite(
            EntityId.NewId(),
            "Test Machine",
            Seq1(((ComposableEntity)indicatorEntity, Coordinate.Identity)),
            Coordinate.Identity);
        
        var ast = Ast.Single(
            new Statement.Action(
                indicatorId,
                new PartAction.Actuator(ActuatorAction.On.Instance)));
        
        // Act
        var result = _validator.Validate(ast, machine);
        
        // Assert
        Assert.True(result.IsRight);
    }
    
    /// <summary>获取机器中第一个指定类型的实体 ID</summary>
    private static EntityId GetFirstEntityOfType<TPartType>(ComposableEntity machine)
        where TPartType : PartType
    {
        return machine switch
        {
            ComposableEntity.Part p when p.PartData.PartType is TPartType => p.Id,
            ComposableEntity.Composite c => c.Children
                .Map(child => GetFirstEntityOfTypeOrNone<TPartType>(child.Entity))
                .Find(id => id.IsSome)
                .Flatten()
                .IfNone(() => throw new InvalidOperationException($"No entity of type {typeof(TPartType).Name} found")),
            _ => throw new InvalidOperationException($"No entity of type {typeof(TPartType).Name} found")
        };
    }
    
    /// <summary>获取实体中第一个指定类型的实体 ID（返回 Option）</summary>
    private static Option<EntityId> GetFirstEntityOfTypeOrNone<TPartType>(ComposableEntity entity)
        where TPartType : PartType
    {
        return entity switch
        {
            ComposableEntity.Part p when p.PartData.PartType is TPartType => Some(p.Id),
            ComposableEntity.Composite c => c.Children
                .Map(child => GetFirstEntityOfTypeOrNone<TPartType>(child.Entity))
                .Find(id => id.IsSome)
                .Flatten(),
            _ => None
        };
    }
}
