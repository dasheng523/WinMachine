using System;
using LanguageExt;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Dsl.Ast;
using MachineOrchestration.Dsl.Interpreter;
using Xunit;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// DSL 解释器的单元测试
/// 验证：需求 15.1-15.6
/// </summary>
public class DslInterpreterTests
{
    private readonly IDslInterpreter _interpreter = new DslInterpreter();
    
    #region Action 执行测试
    
    /// <summary>
    /// 测试 Action 执行 - 电机动作
    /// </summary>
    [Fact]
    public void Step_ActionStatement_MotorAction_UpdatesStateAndAdvancesCounter()
    {
        // Arrange
        var entityId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            entityId,
            new PartState.Motor(0, 0, false, false));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.Action(
            entityId,
            new PartAction.Motor(new MotorAction.MoveTo(100, 50))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                
                var partState = newState.MachineState.GetPartState(entityId);
                Assert.True(partState.IsSome);
                
                partState.IfSome(ps =>
                {
                    Assert.IsType<PartState.Motor>(ps);
                    var motor = (PartState.Motor)ps;
                    Assert.Equal(100, motor.CurrentPosition);
                    Assert.Equal(50, motor.CurrentSpeed);
                    Assert.True(motor.IsMoving);
                });
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 Action 执行 - 执行器动作
    /// </summary>
    [Fact]
    public void Step_ActionStatement_ActuatorAction_UpdatesStateAndAdvancesCounter()
    {
        // Arrange
        var entityId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            entityId,
            new PartState.Actuator(ActuatorStateValue.Retracted, false));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.Action(
            entityId,
            new PartAction.Actuator(ActuatorAction.Extend.Instance)));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                var partState = newState.MachineState.GetPartState(entityId);
                Assert.True(partState.IsSome);
                
                partState.IfSome(ps =>
                {
                    Assert.IsType<PartState.Actuator>(ps);
                    var actuator = (PartState.Actuator)ps;
                    Assert.Equal(ActuatorStateValue.Extended, actuator.State);
                    Assert.True(actuator.IsTransitioning);
                });
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 Action 执行 - 实体不存在时仍然创建默认状态
    /// </summary>
    [Fact]
    public void Step_ActionStatement_EntityNotFound_CreatesDefaultState()
    {
        // Arrange
        var entityId = new EntityId(Guid.NewGuid());
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Action(
            entityId,
            new PartAction.Motor(MotorAction.Stop.Instance)));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                // 应该创建默认的电机状态
                var partState = newState.MachineState.GetPartState(entityId);
                Assert.True(partState.IsSome);
                
                partState.IfSome(ps =>
                {
                    Assert.IsType<PartState.Motor>(ps);
                });
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region Wait 执行测试
    
    /// <summary>
    /// 测试 Wait 执行 - 设置等待时间
    /// </summary>
    [Fact]
    public void Step_WaitStatement_SetsWaitUntil()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var duration = TimeSpan.FromSeconds(2);
        var ast = Ast.Single(new Statement.Wait(duration));
        
        var beforeExecution = DateTime.UtcNow;
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.True(newState.WaitUntil.IsSome);
                
                newState.WaitUntil.IfSome(waitUntil =>
                {
                    var expectedWaitUntil = beforeExecution.Add(duration);
                    var difference = (waitUntil - expectedWaitUntil).Duration();
                    Assert.True(difference < TimeSpan.FromMilliseconds(100));
                });
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 Wait 执行 - 等待期间保持状态
    /// </summary>
    [Fact]
    public void Step_WaitStatement_WhileWaiting_MaintainsState()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty)
            .SetWaitUntil(DateTime.UtcNow.AddSeconds(10));
        var ast = Ast.Single(new Statement.Wait(TimeSpan.FromSeconds(1)));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.Equal(state.Counter, newState.Counter);
                Assert.True(newState.WaitUntil.IsSome);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region WaitUntil 执行测试
    
    /// <summary>
    /// 测试 WaitUntil 执行 - 条件为真时前进
    /// </summary>
    [Fact]
    public void Step_WaitUntilStatement_ConditionTrue_AdvancesCounter()
    {
        // Arrange
        var sensorId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            sensorId,
            new PartState.Sensor(
                Some<SensorReading>(new SensorReading.Boolean(true)),
                DateTime.UtcNow));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.WaitUntil(
            new Condition.SensorState(sensorId, true)));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 WaitUntil 执行 - 条件为假时保持状态
    /// </summary>
    [Fact]
    public void Step_WaitUntilStatement_ConditionFalse_MaintainsState()
    {
        // Arrange
        var sensorId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            sensorId,
            new PartState.Sensor(
                Some<SensorReading>(new SensorReading.Boolean(false)),
                DateTime.UtcNow));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.WaitUntil(
            new Condition.SensorState(sensorId, true)));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.Equal(state.Counter, newState.Counter);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region Sequence 执行测试
    
    /// <summary>
    /// 测试 Sequence 执行 - 进入第一条语句
    /// </summary>
    [Fact]
    public void Step_SequenceStatement_EntersFirstStatement()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Sequence(Seq<Statement>(
            (Statement)new Statement.Wait(TimeSpan.FromSeconds(1)),
            (Statement)new Statement.Wait(TimeSpan.FromSeconds(2)))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                Assert.Equal(1, newState.Counter.Path.Count);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 Sequence 执行 - 空序列标记完成
    /// </summary>
    [Fact]
    public void Step_SequenceStatement_Empty_MarksComplete()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Sequence(Seq<Statement>()));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.True(newState.IsComplete);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region Parallel 执行测试
    
    /// <summary>
    /// 测试 Parallel 执行 - 前进计数器
    /// </summary>
    [Fact]
    public void Step_ParallelStatement_AdvancesCounter()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Parallel(Seq<Statement>(
            (Statement)new Statement.Wait(TimeSpan.FromSeconds(1)),
            (Statement)new Statement.Wait(TimeSpan.FromSeconds(2)))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region Loop 执行测试
    
    /// <summary>
    /// 测试 Loop 执行 - 有限循环进入循环体
    /// </summary>
    [Fact]
    public void Step_LoopStatement_FiniteLoop_EntersBody()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Loop(
            Some<uint>(3),
            new Statement.Wait(TimeSpan.FromSeconds(1))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                Assert.Equal(1, newState.Counter.Path.Count);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 Loop 执行 - 无限循环进入循环体
    /// </summary>
    [Fact]
    public void Step_LoopStatement_InfiniteLoop_EntersBody()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Loop(
            None,
            new Statement.Wait(TimeSpan.FromSeconds(1))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                Assert.Equal(1, newState.Counter.Path.Count);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 Loop 执行 - 计数为零时前进
    /// </summary>
    [Fact]
    public void Step_LoopStatement_ZeroCount_AdvancesCounter()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        var ast = Ast.Single(new Statement.Loop(
            Some<uint>(0),
            new Statement.Wait(TimeSpan.FromSeconds(1))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                // 前进到下一条语句，路径应该是 [1]
                Assert.Equal(1, newState.Counter.Path.Count);
                Assert.Equal(1, newState.Counter.Path[0]);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region If 执行测试
    
    /// <summary>
    /// 测试 If 执行 - 条件为真时进入 then 分支
    /// </summary>
    [Fact]
    public void Step_IfStatement_ConditionTrue_EntersThenBranch()
    {
        // Arrange
        var sensorId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            sensorId,
            new PartState.Sensor(
                Some<SensorReading>(new SensorReading.Boolean(true)),
                DateTime.UtcNow));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.If(
            new Condition.SensorState(sensorId, true),
            new Statement.Wait(TimeSpan.FromSeconds(1)),
            Some<Statement>(new Statement.Wait(TimeSpan.FromSeconds(2)))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                Assert.Equal(1, newState.Counter.Path.Count);
                Assert.Equal(0, newState.Counter.Path[0]);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 If 执行 - 条件为假时进入 else 分支
    /// </summary>
    [Fact]
    public void Step_IfStatement_ConditionFalse_EntersElseBranch()
    {
        // Arrange
        var sensorId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            sensorId,
            new PartState.Sensor(
                Some<SensorReading>(new SensorReading.Boolean(false)),
                DateTime.UtcNow));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.If(
            new Condition.SensorState(sensorId, true),
            new Statement.Wait(TimeSpan.FromSeconds(1)),
            Some<Statement>(new Statement.Wait(TimeSpan.FromSeconds(2)))));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                Assert.Equal(1, newState.Counter.Path.Count);
                Assert.Equal(1, newState.Counter.Path[0]);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    /// <summary>
    /// 测试 If 执行 - 条件为假且无 else 分支时前进
    /// </summary>
    [Fact]
    public void Step_IfStatement_ConditionFalse_NoElse_AdvancesCounter()
    {
        // Arrange
        var sensorId = new EntityId(Guid.NewGuid());
        var initialMachineState = MachineState.Empty.UpdatePartState(
            sensorId,
            new PartState.Sensor(
                Some<SensorReading>(new SensorReading.Boolean(false)),
                DateTime.UtcNow));
        
        var state = ExecutionState.Initial(initialMachineState);
        var ast = Ast.Single(new Statement.If(
            new Condition.SensorState(sensorId, true),
            new Statement.Wait(TimeSpan.FromSeconds(1)),
            None));
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert
        result.Match(
            Right: newState =>
            {
                Assert.NotEqual(state.Counter, newState.Counter);
                // 前进到下一条语句，路径应该是 [1]
                Assert.Equal(1, newState.Counter.Path.Count);
                Assert.Equal(1, newState.Counter.Path[0]);
            },
            Left: error => Assert.Fail($"Expected success but got error: {error.GetMessage()}"));
    }
    
    #endregion
    
    #region IsComplete 测试
    
    /// <summary>
    /// 测试 IsComplete - 已完成状态
    /// </summary>
    [Fact]
    public void IsComplete_CompletedState_ReturnsTrue()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty).MarkComplete();
        
        // Act
        var result = _interpreter.IsComplete(state);
        
        // Assert
        Assert.True(result);
    }
    
    /// <summary>
    /// 测试 IsComplete - 未完成状态
    /// </summary>
    [Fact]
    public void IsComplete_NotCompletedState_ReturnsFalse()
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty);
        
        // Act
        var result = _interpreter.IsComplete(state);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
}

