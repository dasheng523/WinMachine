using System;
using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Dsl.Ast;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// DSL AST 构造单元测试
/// 验证：需求 8.1-8.4
/// </summary>
public class DslAstTests
{
    #region Statement Construction Tests
    
    [Fact]
    public void Action_Statement_ShouldBeCreated()
    {
        // Arrange
        var entityId = EntityId.NewId();
        var motorAction = new MotorAction.MoveTo(100.0f, 50.0f);
        var partAction = new PartAction.Motor(motorAction);
        
        // Act
        var statement = new Statement.Action(entityId, partAction);
        
        // Assert
        Assert.NotNull(statement);
        Assert.Equal(entityId, statement.EntityId);
        Assert.Equal(partAction, statement.PartAction);
    }
    
    [Fact]
    public void Wait_Statement_ShouldBeCreated()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(5);
        
        // Act
        var statement = new Statement.Wait(duration);
        
        // Assert
        Assert.NotNull(statement);
        Assert.Equal(duration, statement.Duration);
    }
    
    [Fact]
    public void WaitUntil_Statement_ShouldBeCreated()
    {
        // Arrange
        var sensorId = EntityId.NewId();
        var condition = new Condition.SensorState(sensorId, true);
        
        // Act
        var statement = new Statement.WaitUntil(condition);
        
        // Assert
        Assert.NotNull(statement);
        Assert.Equal(condition, statement.Condition);
    }
    
    [Fact]
    public void Sequence_Statement_ShouldBeCreated()
    {
        // Arrange
        var wait1 = new Statement.Wait(TimeSpan.FromSeconds(1));
        var wait2 = new Statement.Wait(TimeSpan.FromSeconds(2));
        var statements = Seq<Statement>(wait1, wait2);
        
        // Act
        var sequence = new Statement.Sequence(statements);
        
        // Assert
        Assert.NotNull(sequence);
        Assert.Equal(2, sequence.Statements.Count);
        Assert.Equal(wait1, sequence.Statements[0]);
        Assert.Equal(wait2, sequence.Statements[1]);
    }
    
    [Fact]
    public void Parallel_Statement_ShouldBeCreated()
    {
        // Arrange
        var action1 = new Statement.Action(
            EntityId.NewId(),
            new PartAction.Motor(MotorAction.Home.Instance));
        var action2 = new Statement.Action(
            EntityId.NewId(),
            new PartAction.Actuator(ActuatorAction.Extend.Instance));
        var statements = Seq<Statement>(action1, action2);
        
        // Act
        var parallel = new Statement.Parallel(statements);
        
        // Assert
        Assert.NotNull(parallel);
        Assert.Equal(2, parallel.Statements.Count);
    }
    
    [Fact]
    public void Loop_Statement_WithCount_ShouldBeCreated()
    {
        // Arrange
        var body = new Statement.Wait(TimeSpan.FromSeconds(1));
        var count = Some<uint>(10);
        
        // Act
        var loop = new Statement.Loop(count, body);
        
        // Assert
        Assert.NotNull(loop);
        Assert.True(loop.Count.IsSome);
        Assert.Equal(10u, loop.Count.IfNone(0u));
        Assert.Equal(body, loop.Body);
    }
    
    [Fact]
    public void Loop_Statement_Infinite_ShouldBeCreated()
    {
        // Arrange
        var body = new Statement.Wait(TimeSpan.FromSeconds(1));
        var count = Option<uint>.None;
        
        // Act
        var loop = new Statement.Loop(count, body);
        
        // Assert
        Assert.NotNull(loop);
        Assert.True(loop.Count.IsNone);
        Assert.Equal(body, loop.Body);
    }
    
    [Fact]
    public void If_Statement_WithElse_ShouldBeCreated()
    {
        // Arrange
        var condition = new Condition.SensorState(EntityId.NewId(), true);
        Statement thenBranch = new Statement.Wait(TimeSpan.FromSeconds(1));
        Statement elseBranch = new Statement.Wait(TimeSpan.FromSeconds(2));
        
        // Act
        var ifStatement = new Statement.If(condition, thenBranch, Some(elseBranch));
        
        // Assert
        Assert.NotNull(ifStatement);
        Assert.Equal(condition, ifStatement.Condition);
        Assert.Equal(thenBranch, ifStatement.ThenBranch);
        Assert.True(ifStatement.ElseBranch.IsSome);
        Assert.Equal(elseBranch, ifStatement.ElseBranch.IfNone(() => null!));
    }
    
    [Fact]
    public void If_Statement_WithoutElse_ShouldBeCreated()
    {
        // Arrange
        var condition = new Condition.SensorState(EntityId.NewId(), true);
        var thenBranch = new Statement.Wait(TimeSpan.FromSeconds(1));
        
        // Act
        var ifStatement = new Statement.If(condition, thenBranch, Option<Statement>.None);
        
        // Assert
        Assert.NotNull(ifStatement);
        Assert.Equal(condition, ifStatement.Condition);
        Assert.Equal(thenBranch, ifStatement.ThenBranch);
        Assert.True(ifStatement.ElseBranch.IsNone);
    }
    
    #endregion
    
    #region Condition Construction Tests
    
    [Fact]
    public void SensorState_Condition_ShouldBeCreated()
    {
        // Arrange
        var sensorId = EntityId.NewId();
        var expected = true;
        
        // Act
        var condition = new Condition.SensorState(sensorId, expected);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(sensorId, condition.SensorId);
        Assert.Equal(expected, condition.Expected);
    }
    
    [Fact]
    public void StateSensor_Condition_ShouldBeCreated()
    {
        // Arrange
        var sensorId = StateSensorId.NewId();
        var expected = true;
        
        // Act
        var condition = new Condition.StateSensor(sensorId, expected);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(sensorId, condition.SensorId);
        Assert.Equal(expected, condition.Expected);
    }
    
    [Fact]
    public void SensorValue_Condition_ShouldBeCreated()
    {
        // Arrange
        var sensorId = EntityId.NewId();
        var op = ComparisonOp.Greater;
        var value = 100.0f;
        
        // Act
        var condition = new Condition.SensorValue(sensorId, op, value);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(sensorId, condition.SensorId);
        Assert.Equal(op, condition.Operator);
        Assert.Equal(value, condition.Value);
    }
    
    [Fact]
    public void And_Condition_ShouldBeCreated()
    {
        // Arrange
        var left = new Condition.SensorState(EntityId.NewId(), true);
        var right = new Condition.SensorState(EntityId.NewId(), false);
        
        // Act
        var condition = new Condition.And(left, right);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(left, condition.Left);
        Assert.Equal(right, condition.Right);
    }
    
    [Fact]
    public void Or_Condition_ShouldBeCreated()
    {
        // Arrange
        var left = new Condition.SensorState(EntityId.NewId(), true);
        var right = new Condition.SensorState(EntityId.NewId(), false);
        
        // Act
        var condition = new Condition.Or(left, right);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(left, condition.Left);
        Assert.Equal(right, condition.Right);
    }
    
    [Fact]
    public void Not_Condition_ShouldBeCreated()
    {
        // Arrange
        var inner = new Condition.SensorState(EntityId.NewId(), true);
        
        // Act
        var condition = new Condition.Not(inner);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(inner, condition.Inner);
    }
    
    [Theory]
    [InlineData(ComparisonOp.Equal)]
    [InlineData(ComparisonOp.NotEqual)]
    [InlineData(ComparisonOp.Greater)]
    [InlineData(ComparisonOp.GreaterOrEqual)]
    [InlineData(ComparisonOp.Less)]
    [InlineData(ComparisonOp.LessOrEqual)]
    public void ComparisonOp_AllValues_ShouldBeValid(ComparisonOp op)
    {
        // Arrange & Act
        var condition = new Condition.SensorValue(EntityId.NewId(), op, 0.0f);
        
        // Assert
        Assert.NotNull(condition);
        Assert.Equal(op, condition.Operator);
    }
    
    #endregion
    
    #region Ast Construction Tests
    
    [Fact]
    public void Ast_Empty_ShouldBeCreated()
    {
        // Act
        var ast = Ast.Empty;
        
        // Assert
        Assert.NotNull(ast);
        Assert.Empty(ast.Statements);
    }
    
    [Fact]
    public void Ast_Single_ShouldBeCreated()
    {
        // Arrange
        var statement = new Statement.Wait(TimeSpan.FromSeconds(1));
        
        // Act
        var ast = Ast.Single(statement);
        
        // Assert
        Assert.NotNull(ast);
        Assert.Single(ast.Statements);
        Assert.Equal(statement, ast.Statements[0]);
    }
    
    [Fact]
    public void Ast_Create_WithMultipleStatements_ShouldBeCreated()
    {
        // Arrange
        var stmt1 = new Statement.Wait(TimeSpan.FromSeconds(1));
        var stmt2 = new Statement.Wait(TimeSpan.FromSeconds(2));
        var stmt3 = new Statement.Wait(TimeSpan.FromSeconds(3));
        
        // Act
        var ast = Ast.Create(stmt1, stmt2, stmt3);
        
        // Assert
        Assert.NotNull(ast);
        Assert.Equal(3, ast.Statements.Count);
        Assert.Equal(stmt1, ast.Statements[0]);
        Assert.Equal(stmt2, ast.Statements[1]);
        Assert.Equal(stmt3, ast.Statements[2]);
    }
    
    [Fact]
    public void Ast_Constructor_WithSeq_ShouldBeCreated()
    {
        // Arrange
        var statements = Seq<Statement>(
            new Statement.Wait(TimeSpan.FromSeconds(1)),
            new Statement.Wait(TimeSpan.FromSeconds(2))
        );
        
        // Act
        var ast = new Ast(statements);
        
        // Assert
        Assert.NotNull(ast);
        Assert.Equal(2, ast.Statements.Count);
    }
    
    #endregion
    
    #region Nested Structure Tests
    
    [Fact]
    public void NestedSequence_ShouldBeCreated()
    {
        // Arrange
        var innerSequence = new Statement.Sequence(Seq<Statement>(
            new Statement.Wait(TimeSpan.FromSeconds(1)),
            new Statement.Wait(TimeSpan.FromSeconds(2))
        ));
        
        var outerSequence = new Statement.Sequence(Seq<Statement>(
            innerSequence,
            new Statement.Wait(TimeSpan.FromSeconds(3))
        ));
        
        // Act & Assert
        Assert.NotNull(outerSequence);
        Assert.Equal(2, outerSequence.Statements.Count);
        
        var nested = outerSequence.Statements[0] as Statement.Sequence;
        Assert.NotNull(nested);
        Assert.Equal(2, nested.Statements.Count);
    }
    
    [Fact]
    public void LoopWithSequence_ShouldBeCreated()
    {
        // Arrange
        var sequence = new Statement.Sequence(Seq<Statement>(
            new Statement.Action(
                EntityId.NewId(),
                new PartAction.Motor(new MotorAction.MoveTo(100.0f, 50.0f))),
            new Statement.Wait(TimeSpan.FromSeconds(1))
        ));
        
        var loop = new Statement.Loop(Some<uint>(5), sequence);
        
        // Act & Assert
        Assert.NotNull(loop);
        Assert.Equal(5u, loop.Count.IfNone(0u));
        
        var body = loop.Body as Statement.Sequence;
        Assert.NotNull(body);
        Assert.Equal(2, body.Statements.Count);
    }
    
    [Fact]
    public void IfWithNestedConditions_ShouldBeCreated()
    {
        // Arrange
        var sensor1 = EntityId.NewId();
        var sensor2 = EntityId.NewId();
        
        var condition = new Condition.And(
            new Condition.SensorState(sensor1, true),
            new Condition.Or(
                new Condition.SensorState(sensor2, false),
                new Condition.SensorValue(sensor1, ComparisonOp.Greater, 50.0f)
            )
        );
        
        var ifStatement = new Statement.If(
            condition,
            new Statement.Wait(TimeSpan.FromSeconds(1)),
            Option<Statement>.None
        );
        
        // Act & Assert
        Assert.NotNull(ifStatement);
        
        var andCondition = ifStatement.Condition as Condition.And;
        Assert.NotNull(andCondition);
        
        var orCondition = andCondition.Right as Condition.Or;
        Assert.NotNull(orCondition);
    }
    
    [Fact]
    public void ComplexNestedStructure_ShouldBeCreated()
    {
        // Arrange - 创建一个复杂的嵌套结构
        var motorId = EntityId.NewId();
        var sensorId = EntityId.NewId();
        
        var ast = Ast.Create(
            new Statement.Sequence(Seq<Statement>(
                new Statement.Action(
                    motorId,
                    new PartAction.Motor(MotorAction.Home.Instance)),
                new Statement.WaitUntil(
                    new Condition.SensorState(sensorId, true)),
                new Statement.Loop(
                    Some<uint>(3),
                    new Statement.Parallel(Seq<Statement>(
                        new Statement.Action(
                            motorId,
                            new PartAction.Motor(new MotorAction.MoveTo(100.0f, 50.0f))),
                        new Statement.Wait(TimeSpan.FromSeconds(2))
                    ))
                )
            ))
        );
        
        // Act & Assert
        Assert.NotNull(ast);
        Assert.Single(ast.Statements);
        
        var sequence = ast.Statements[0] as Statement.Sequence;
        Assert.NotNull(sequence);
        Assert.Equal(3, sequence.Statements.Count);
        
        var loop = sequence.Statements[2] as Statement.Loop;
        Assert.NotNull(loop);
        
        var parallel = loop.Body as Statement.Parallel;
        Assert.NotNull(parallel);
        Assert.Equal(2, parallel.Statements.Count);
    }
    
    #endregion
}
