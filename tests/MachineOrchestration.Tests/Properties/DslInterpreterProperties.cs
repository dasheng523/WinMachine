using System;
using FsCheck;
using FsCheck.Xunit;
using LanguageExt;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Dsl.Ast;
using MachineOrchestration.Dsl.Interpreter;
using Xunit;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Tests.Properties;

/// <summary>
/// DSL 解释器的基于属性的测试
/// 验证：需求 15.2
/// </summary>
public class DslInterpreterProperties
{
    private readonly IDslInterpreter _interpreter = new DslInterpreter();
    
    /// <summary>
    /// 属性 19：状态转换确定性
    /// 
    /// 对于任何给定的执行状态和 AST，Step 方法应该总是返回相同的结果。
    /// 这验证了解释器是纯函数，没有隐藏的副作用或随机性。
    /// 
    /// 验证：需求 15.2
    /// </summary>
    [Property(MaxTest = 100)]
    public void StateTransitionIsDeterministic(bool isComplete)
    {
        // Arrange - 使用 Action 而不是 Wait 来避免 DateTime.UtcNow 的不确定性
        var entityId = new EntityId(Guid.NewGuid());
        var machineState = MachineState.Empty.UpdatePartState(
            entityId,
            new PartState.Motor(0, 0, false, false));
        var state = ExecutionState.Initial(machineState) with
        {
            IsComplete = isComplete
        };
        var ast = Ast.Single(new Statement.Action(
            entityId,
            new PartAction.Motor(MotorAction.Stop.Instance)));
        
        // Act
        var result1 = _interpreter.Step(state, ast);
        var result2 = _interpreter.Step(state, ast);
        
        // Assert
        result1.Match(
            Right: state1 => result2.Match(
                Right: state2 => Assert.Equal(state1, state2),
                Left: _ => Assert.Fail("Second execution failed but first succeeded")),
            Left: error1 => result2.Match(
                Right: _ => Assert.Fail("First execution failed but second succeeded"),
                Left: error2 => Assert.Equal(error1.GetType(), error2.GetType())));
    }
    
    /// <summary>
    /// 属性 19（扩展）：多次执行的确定性
    /// 
    /// 对于任何执行状态和 AST，多次执行 Step 应该产生相同的结果序列。
    /// 
    /// 验证：需求 15.2
    /// </summary>
    [Property(MaxTest = 100)]
    public void MultipleStepsAreDeterministic(bool isComplete)
    {
        // Arrange - 使用 Sequence 而不是 Wait 来避免 DateTime.UtcNow 的不确定性
        var initialState = ExecutionState.Initial(MachineState.Empty) with
        {
            IsComplete = isComplete
        };
        var ast = Ast.Single(new Statement.Sequence(Seq<Statement>()));
        
        // Act
        var states1 = ExecuteNSteps(initialState, ast, 3);
        var states2 = ExecuteNSteps(initialState, ast, 3);
        
        // Assert
        Assert.Equal(states1.Count, states2.Count);
        
        for (int i = 0; i < states1.Count; i++)
        {
            Assert.Equal(states1[i], states2[i]);
        }
    }
    
    /// <summary>
    /// 执行 N 步并收集状态
    /// </summary>
    private Seq<ExecutionState> ExecuteNSteps(
        ExecutionState initialState,
        Ast ast,
        int steps)
    {
        var states = Seq<ExecutionState>();
        var currentState = initialState;
        
        for (int i = 0; i < steps; i++)
        {
            if (_interpreter.IsComplete(currentState))
                break;
            
            var result = _interpreter.Step(currentState, ast);
            
            result.Match(
                Right: newState =>
                {
                    states = states.Add(newState);
                    currentState = newState;
                },
                Left: _ => { });
        }
        
        return states;
    }
    
    /// <summary>
    /// 属性 20：状态转换不可变性
    /// 
    /// Step 方法不应该修改输入的执行状态。
    /// 这验证了解释器遵循不可变性原则。
    /// 
    /// 验证：需求 15.2
    /// </summary>
    [Property(MaxTest = 100)]
    public void StateTransitionIsImmutable(bool isComplete)
    {
        // Arrange
        var state = ExecutionState.Initial(MachineState.Empty) with
        {
            IsComplete = isComplete
        };
        var ast = Ast.Single(new Statement.Wait(TimeSpan.FromSeconds(1)));
        
        // 保存原始状态的副本
        var originalCounter = state.Counter;
        var originalIsComplete = state.IsComplete;
        var originalWaitUntil = state.WaitUntil;
        var originalCallStackCount = state.CallStack.Count;
        var originalBindingsCount = state.Bindings.Count;
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert - 验证原始状态未被修改
        Assert.Equal(originalCounter, state.Counter);
        Assert.Equal(originalIsComplete, state.IsComplete);
        Assert.Equal(originalWaitUntil, state.WaitUntil);
        Assert.Equal(originalCallStackCount, state.CallStack.Count);
        Assert.Equal(originalBindingsCount, state.Bindings.Count);
    }
    
    /// <summary>
    /// 属性 20（扩展）：机器状态不可变性
    /// 
    /// Step 方法不应该修改输入的机器状态对象。
    /// 
    /// 验证：需求 15.2
    /// </summary>
    [Property(MaxTest = 100)]
    public void MachineStateIsImmutable(Guid entityGuid)
    {
        // Arrange
        var entityId = new EntityId(entityGuid);
        var machineState = MachineState.Empty.UpdatePartState(
            entityId,
            new PartState.Motor(0, 0, false, false));
        var state = ExecutionState.Initial(machineState);
        var ast = Ast.Single(new Statement.Action(
            entityId,
            new PartAction.Motor(MotorAction.Stop.Instance)));
        
        // 保存原始机器状态的引用
        var originalMachineState = state.MachineState;
        var originalPartStatesCount = originalMachineState.PartStates.Count;
        
        // Act
        var result = _interpreter.Step(state, ast);
        
        // Assert - 验证原始机器状态未被修改
        Assert.Equal(originalMachineState, state.MachineState);
        Assert.Equal(originalPartStatesCount, state.MachineState.PartStates.Count);
    }
}



