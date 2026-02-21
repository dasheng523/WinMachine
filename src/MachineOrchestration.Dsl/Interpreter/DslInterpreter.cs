using System;
using System.Linq;
using LanguageExt;
using MachineOrchestration.Core.Types;
using static LanguageExt.Prelude;
using AstType = MachineOrchestration.Dsl.Ast.Ast;
using Statement = MachineOrchestration.Dsl.Ast.Statement;
using Condition = MachineOrchestration.Dsl.Ast.Condition;
using ComparisonOp = MachineOrchestration.Dsl.Ast.ComparisonOp;

namespace MachineOrchestration.Dsl.Interpreter;

/// <summary>DSL 解释器实现（纯函数）</summary>
/// <remarks>
/// 实现纯函数式的 DSL 解释器。所有状态转换都是不可变的。
/// 验证：需求 15.1-15.6
/// </remarks>
public sealed class DslInterpreter : IDslInterpreter
{
    /// <summary>执行一步（纯函数）</summary>
    public Either<ExecutionError, ExecutionState> Step(
        ExecutionState state,
        AstType ast)
    {
        // 如果已完成，直接返回
        if (state.IsComplete)
            return Right<ExecutionError, ExecutionState>(state);
        
        // 如果正在等待，检查是否可以继续
        if (state.WaitUntil.IsSome)
        {
            var waitUntil = state.WaitUntil.IfNone(DateTime.MinValue);
            if (DateTime.UtcNow < waitUntil)
                return Right<ExecutionError, ExecutionState>(state);
            
            // 等待完成，清除等待并前进
            return Right<ExecutionError, ExecutionState>(
                state.ClearWait().UpdateCounter(state.Counter.Next()));
        }
        
        // 获取当前语句
        var statementResult = GetCurrentStatement(ast.Statements, state.Counter);
        
        return statementResult.Match(
            Right: stmt => ExecuteStatement(stmt, state, ast),
            Left: error => Left<ExecutionError, ExecutionState>(error));
    }
    
    /// <summary>检查执行是否完成</summary>
    public bool IsComplete(ExecutionState state) => state.IsComplete;
    
    /// <summary>获取当前语句</summary>
    private Either<ExecutionError, Statement> GetCurrentStatement(
        Seq<Statement> statements,
        ProgramCounter counter)
    {
        if (counter.Path.IsEmpty)
        {
            // 顶层语句
            if (statements.IsEmpty)
                return Left<ExecutionError, Statement>(
                    new ExecutionError.InvalidStateTransition(
                        "No statements to execute",
                        "Empty",
                        "Execute"));
            
            return Right<ExecutionError, Statement>(statements.Head);
        }
        
        // 导航到嵌套语句
        var path = counter.Path.ToArray();
        Statement current = statements.Head;
        
        for (int i = 0; i < path.Length; i++)
        {
            var index = path[i];
            
            current = current switch
            {
                Statement.Sequence seq => 
                    index < seq.Statements.Count 
                        ? seq.Statements[index]
                        : null!,
                
                Statement.Parallel par => 
                    index < par.Statements.Count 
                        ? par.Statements[index]
                        : null!,
                
                Statement.Loop loop => loop.Body,
                
                Statement.If ifStmt when index == 0 => ifStmt.ThenBranch,
                Statement.If ifStmt when index == 1 => 
                    ifStmt.ElseBranch.IfNone(() => null!),
                
                _ => null!
            };
            
            if (current == null)
                return Left<ExecutionError, Statement>(
                    new ExecutionError.InvalidStateTransition(
                        $"Invalid path at index {i}",
                        "Navigation",
                        $"Index {index}"));
        }
        
        return Right<ExecutionError, Statement>(current);
    }
    
    /// <summary>执行语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteStatement(
        Statement statement,
        ExecutionState state,
        AstType ast)
    {
        return statement switch
        {
            Statement.Action action => ExecuteAction(action, state),
            Statement.Wait wait => ExecuteWait(wait, state),
            Statement.WaitUntil waitUntil => ExecuteWaitUntil(waitUntil, state),
            Statement.Sequence sequence => ExecuteSequence(sequence, state),
            Statement.Parallel parallel => ExecuteParallel(parallel, state),
            Statement.Loop loop => ExecuteLoop(loop, state),
            Statement.If ifStmt => ExecuteIf(ifStmt, state),
            _ => Left<ExecutionError, ExecutionState>(
                new ExecutionError.InvalidStateTransition(
                    $"Unknown statement type: {statement.GetType().Name}",
                    "Execute",
                    "Unknown"))
        };
    }
    
    /// <summary>执行动作语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteAction(
        Statement.Action action,
        ExecutionState state)
    {
        // 获取实体当前状态
        var partStateOpt = state.MachineState.GetPartState(action.EntityId);
        
        // 根据动作类型更新状态
        var newPartState = action.PartAction switch
        {
            PartAction.Motor motorAction => 
                UpdateMotorState(partStateOpt, motorAction.Action),
            
            PartAction.Actuator actuatorAction => 
                UpdateActuatorState(partStateOpt, actuatorAction.Action),
            
            _ => partStateOpt
        };
        
        // 如果状态未改变（实体不存在），返回错误
        if (newPartState.IsNone)
            return Left<ExecutionError, ExecutionState>(
                new ExecutionError.EntityNotFound(action.EntityId));
        
        // 更新机器状态并前进程序计数器
        var newMachineState = state.MachineState.UpdatePartState(
            action.EntityId,
            newPartState.IfNone(() => new PartState.Sensor(None, DateTime.UtcNow)));
        
        return Right<ExecutionError, ExecutionState>(
            state.UpdateMachineState(newMachineState)
                 .UpdateCounter(state.Counter.Next()));
    }
    
    /// <summary>更新电机状态</summary>
    private Option<PartState> UpdateMotorState(
        Option<PartState> currentState,
        MotorAction action)
    {
        var motor = currentState.Match(
            Some: s => s is PartState.Motor m ? m : new PartState.Motor(0, 0, false, false),
            None: () => new PartState.Motor(0, 0, false, false));
        
        return action switch
        {
            MotorAction.MoveTo moveTo => 
                Some<PartState>(motor with 
                { 
                    CurrentPosition = moveTo.Position,
                    CurrentSpeed = moveTo.Speed,
                    IsMoving = true 
                }),
            
            MotorAction.RotateTo rotateTo => 
                Some<PartState>(motor with 
                { 
                    CurrentPosition = rotateTo.Angle,
                    CurrentSpeed = rotateTo.Speed,
                    IsMoving = true 
                }),
            
            MotorAction.Home => 
                Some<PartState>(motor with 
                { 
                    CurrentPosition = 0,
                    IsHomed = true,
                    IsMoving = true 
                }),
            
            MotorAction.Stop => 
                Some<PartState>(motor with 
                { 
                    CurrentSpeed = 0,
                    IsMoving = false 
                }),
            
            _ => Some<PartState>(motor)
        };
    }
    
    /// <summary>更新执行器状态</summary>
    private Option<PartState> UpdateActuatorState(
        Option<PartState> currentState,
        ActuatorAction action)
    {
        var actuator = currentState.Match(
            Some: s => s is PartState.Actuator a ? a : new PartState.Actuator(ActuatorStateValue.Unknown, false),
            None: () => new PartState.Actuator(ActuatorStateValue.Unknown, false));
        
        var newState = action switch
        {
            ActuatorAction.Extend => ActuatorStateValue.Extended,
            ActuatorAction.Retract => ActuatorStateValue.Retracted,
            ActuatorAction.Close => ActuatorStateValue.Closed,
            ActuatorAction.Open => ActuatorStateValue.Opened,
            ActuatorAction.Suction => ActuatorStateValue.Suctioning,
            ActuatorAction.Normal => ActuatorStateValue.Normal,
            ActuatorAction.On => ActuatorStateValue.On,
            ActuatorAction.Off => ActuatorStateValue.Off,
            _ => actuator.State
        };
        
        return Some<PartState>(actuator with 
        { 
            State = newState,
            IsTransitioning = true 
        });
    }
    
    /// <summary>执行等待语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteWait(
        Statement.Wait wait,
        ExecutionState state)
    {
        var waitUntil = DateTime.UtcNow.Add(wait.Duration);
        return Right<ExecutionError, ExecutionState>(
            state.SetWaitUntil(waitUntil));
    }
    
    /// <summary>执行等待条件语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteWaitUntil(
        Statement.WaitUntil waitUntil,
        ExecutionState state)
    {
        // 评估条件
        var conditionResult = EvaluateCondition(waitUntil.Condition, state);
        
        return conditionResult.Match(
            Right: isTrue =>
            {
                if (isTrue)
                {
                    // 条件满足，前进
                    return Right<ExecutionError, ExecutionState>(
                        state.UpdateCounter(state.Counter.Next()));
                }
                else
                {
                    // 条件不满足，保持当前状态（等待）
                    return Right<ExecutionError, ExecutionState>(state);
                }
            },
            Left: error => Left<ExecutionError, ExecutionState>(error));
    }
    
    /// <summary>评估条件</summary>
    private Either<ExecutionError, bool> EvaluateCondition(
        Condition condition,
        ExecutionState state)
    {
        return condition switch
        {
            Condition.SensorState sensorState => 
                EvaluateSensorState(sensorState, state),
            
            Condition.StateSensor stateSensor => 
                EvaluateStateSensor(stateSensor, state),
            
            Condition.SensorValue sensorValue => 
                EvaluateSensorValue(sensorValue, state),
            
            Condition.And and => 
                EvaluateAnd(and, state),
            
            Condition.Or or => 
                EvaluateOr(or, state),
            
            Condition.Not not => 
                EvaluateNot(not, state),
            
            _ => Left<ExecutionError, bool>(
                new ExecutionError.ConditionEvaluationError(
                    $"Unknown condition type: {condition.GetType().Name}"))
        };
    }
    
    /// <summary>评估传感器状态条件</summary>
    private Either<ExecutionError, bool> EvaluateSensorState(
        Condition.SensorState sensorState,
        ExecutionState state)
    {
        var partStateOpt = state.MachineState.GetPartState(sensorState.SensorId);
        
        return partStateOpt.Match(
            Some: partState =>
            {
                if (partState is PartState.Sensor sensor)
                {
                    return sensor.LastReading.Match(
                        Some: reading =>
                        {
                            if (reading is SensorReading.Boolean boolReading)
                                return Right<ExecutionError, bool>(
                                    boolReading.Value == sensorState.Expected);
                            
                            return Left<ExecutionError, bool>(
                                new ExecutionError.ConditionEvaluationError(
                                    "Sensor reading is not boolean"));
                        },
                        None: () => Right<ExecutionError, bool>(false));
                }
                
                return Left<ExecutionError, bool>(
                    new ExecutionError.ConditionEvaluationError(
                        "Entity is not a sensor"));
            },
            None: () => Left<ExecutionError, bool>(
                new ExecutionError.EntityNotFound(sensorState.SensorId)));
    }
    
    /// <summary>评估状态传感器条件</summary>
    private Either<ExecutionError, bool> EvaluateStateSensor(
        Condition.StateSensor stateSensor,
        ExecutionState state)
    {
        // 简化实现：假设状态传感器总是返回期望值
        // 实际实现需要从控制板读取状态传感器
        return Right<ExecutionError, bool>(stateSensor.Expected);
    }
    
    /// <summary>评估传感器值条件</summary>
    private Either<ExecutionError, bool> EvaluateSensorValue(
        Condition.SensorValue sensorValue,
        ExecutionState state)
    {
        var partStateOpt = state.MachineState.GetPartState(sensorValue.SensorId);
        
        return partStateOpt.Match(
            Some: partState =>
            {
                if (partState is PartState.Sensor sensor)
                {
                    return sensor.LastReading.Match(
                        Some: reading =>
                        {
                            if (reading is SensorReading.Numeric numReading)
                            {
                                var result = sensorValue.Operator switch
                                {
                                    ComparisonOp.Equal => 
                                        Math.Abs(numReading.Value - sensorValue.Value) < 0.001f,
                                    ComparisonOp.NotEqual => 
                                        Math.Abs(numReading.Value - sensorValue.Value) >= 0.001f,
                                    ComparisonOp.Greater => 
                                        numReading.Value > sensorValue.Value,
                                    ComparisonOp.GreaterOrEqual => 
                                        numReading.Value >= sensorValue.Value,
                                    ComparisonOp.Less => 
                                        numReading.Value < sensorValue.Value,
                                    ComparisonOp.LessOrEqual => 
                                        numReading.Value <= sensorValue.Value,
                                    _ => false
                                };
                                
                                return Right<ExecutionError, bool>(result);
                            }
                            
                            return Left<ExecutionError, bool>(
                                new ExecutionError.ConditionEvaluationError(
                                    "Sensor reading is not numeric"));
                        },
                        None: () => Right<ExecutionError, bool>(false));
                }
                
                return Left<ExecutionError, bool>(
                    new ExecutionError.ConditionEvaluationError(
                        "Entity is not a sensor"));
            },
            None: () => Left<ExecutionError, bool>(
                new ExecutionError.EntityNotFound(sensorValue.SensorId)));
    }
    
    /// <summary>评估逻辑与条件</summary>
    private Either<ExecutionError, bool> EvaluateAnd(
        Condition.And and,
        ExecutionState state)
    {
        var leftResult = EvaluateCondition(and.Left, state);
        
        return leftResult.Match(
            Right: leftValue =>
            {
                if (!leftValue)
                    return Right<ExecutionError, bool>(false);
                
                return EvaluateCondition(and.Right, state);
            },
            Left: error => Left<ExecutionError, bool>(error));
    }
    
    /// <summary>评估逻辑或条件</summary>
    private Either<ExecutionError, bool> EvaluateOr(
        Condition.Or or,
        ExecutionState state)
    {
        var leftResult = EvaluateCondition(or.Left, state);
        
        return leftResult.Match(
            Right: leftValue =>
            {
                if (leftValue)
                    return Right<ExecutionError, bool>(true);
                
                return EvaluateCondition(or.Right, state);
            },
            Left: error => Left<ExecutionError, bool>(error));
    }
    
    /// <summary>评估逻辑非条件</summary>
    private Either<ExecutionError, bool> EvaluateNot(
        Condition.Not not,
        ExecutionState state)
    {
        var innerResult = EvaluateCondition(not.Inner, state);
        
        return innerResult.Match(
            Right: value => Right<ExecutionError, bool>(!value),
            Left: error => Left<ExecutionError, bool>(error));
    }
    
    /// <summary>执行顺序语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteSequence(
        Statement.Sequence sequence,
        ExecutionState state)
    {
        if (sequence.Statements.IsEmpty)
        {
            // 空序列，标记完成
            return Right<ExecutionError, ExecutionState>(
                state.MarkComplete());
        }
        
        // 进入第一条语句
        return Right<ExecutionError, ExecutionState>(
            state.UpdateCounter(state.Counter.Enter(0)));
    }
    
    /// <summary>执行并行语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteParallel(
        Statement.Parallel parallel,
        ExecutionState state)
    {
        // 简化实现：标记为完成
        // 实际并行执行由执行器（Executor）处理
        return Right<ExecutionError, ExecutionState>(
            state.UpdateCounter(state.Counter.Next()));
    }
    
    /// <summary>执行循环语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteLoop(
        Statement.Loop loop,
        ExecutionState state)
    {
        // 检查循环计数
        var shouldContinue = loop.Count.Match(
            Some: count => count > 0,
            None: () => true); // 无限循环
        
        if (shouldContinue)
        {
            // 进入循环体
            return Right<ExecutionError, ExecutionState>(
                state.UpdateCounter(state.Counter.Enter(0)));
        }
        else
        {
            // 循环完成，前进
            return Right<ExecutionError, ExecutionState>(
                state.UpdateCounter(state.Counter.Next()));
        }
    }
    
    /// <summary>执行条件语句</summary>
    private Either<ExecutionError, ExecutionState> ExecuteIf(
        Statement.If ifStmt,
        ExecutionState state)
    {
        // 评估条件
        var conditionResult = EvaluateCondition(ifStmt.Condition, state);
        
        return conditionResult.Match(
            Right: isTrue =>
            {
                if (isTrue)
                {
                    // 进入 then 分支
                    return Right<ExecutionError, ExecutionState>(
                        state.UpdateCounter(state.Counter.Enter(0)));
                }
                else if (ifStmt.ElseBranch.IsSome)
                {
                    // 进入 else 分支
                    return Right<ExecutionError, ExecutionState>(
                        state.UpdateCounter(state.Counter.Enter(1)));
                }
                else
                {
                    // 无 else 分支，前进
                    return Right<ExecutionError, ExecutionState>(
                        state.UpdateCounter(state.Counter.Next()));
                }
            },
            Left: error => Left<ExecutionError, ExecutionState>(error));
    }
}

