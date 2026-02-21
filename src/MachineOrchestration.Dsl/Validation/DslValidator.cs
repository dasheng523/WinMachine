using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MachineOrchestration.Core.Types;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Dsl.Validation;

/// <summary>DSL 语义验证器实现（纯函数）</summary>
/// <remarks>
/// 验证 DSL 抽象语法树的语义正确性，包括：
/// - 实体 ID 是否存在于机器中
/// - 传感器引用是否有效
/// - 动作是否与零件类型兼容
/// - 状态传感器引用是否有效（针对执行器）
/// 验证：需求 8.3, 9.2, 28.8
/// </remarks>
public sealed class DslValidator : IDslValidator
{
    /// <summary>验证 AST 的语义正确性</summary>
    public Either<ValidationError, Unit> Validate(Ast.Ast ast, ComposableEntity machine)
    {
        // 构建实体映射表用于快速查找
        var entityMap = BuildEntityMap(machine);
        
        // 收集所有验证错误
        var errors = new List<ValidationError>();
        
        // 验证每个语句
        foreach (var statement in ast.Statements)
        {
            var statementErrors = ValidateStatement(statement, entityMap);
            errors.AddRange(statementErrors);
        }
        
        // 如果有错误，返回 Multiple 错误；否则返回成功
        return errors.Count > 0
            ? Left<ValidationError, Unit>(new ValidationError.Multiple(Seq(errors)))
            : Right<ValidationError, Unit>(unit);
    }
    
    /// <summary>构建实体映射表</summary>
    /// <remarks>递归遍历机器结构，构建 EntityId -> (Part, PartConfig) 的映射</remarks>
    private static Dictionary<EntityId, (Part Part, PartConfig Config)> BuildEntityMap(ComposableEntity entity)
    {
        var map = new Dictionary<EntityId, (Part, PartConfig)>();
        CollectEntities(entity, map);
        return map;
    }
    
    /// <summary>递归收集所有实体</summary>
    private static void CollectEntities(
        ComposableEntity entity,
        Dictionary<EntityId, (Part, PartConfig)> map)
    {
        switch (entity)
        {
            case ComposableEntity.Part p:
                map[p.Id] = (p.PartData, p.Config);
                break;
                
            case ComposableEntity.Composite c:
                foreach (var (child, _) in c.Children)
                {
                    CollectEntities(child, map);
                }
                break;
        }
    }
    
    /// <summary>验证语句</summary>
    private static IEnumerable<ValidationError> ValidateStatement(
        Ast.Statement statement,
        Dictionary<EntityId, (Part Part, PartConfig Config)> entityMap)
    {
        return statement switch
        {
            Ast.Statement.Action action => ValidateAction(action, entityMap),
            Ast.Statement.WaitUntil waitUntil => ValidateCondition(waitUntil.Condition, entityMap),
            Ast.Statement.Sequence seq => seq.Statements.Bind(s => ValidateStatement(s, entityMap)),
            Ast.Statement.Parallel par => par.Statements.Bind(s => ValidateStatement(s, entityMap)),
            Ast.Statement.Loop loop => ValidateStatement(loop.Body, entityMap),
            Ast.Statement.If ifStmt => ValidateIfStatement(ifStmt, entityMap),
            Ast.Statement.Wait _ => Enumerable.Empty<ValidationError>(),
            _ => Seq1(new ValidationError.InvalidCondition("Unknown statement type"))
        };
    }
    
    /// <summary>验证动作语句</summary>
    private static IEnumerable<ValidationError> ValidateAction(
        Ast.Statement.Action action,
        Dictionary<EntityId, (Part Part, PartConfig Config)> entityMap)
    {
        // 检查实体是否存在
        if (!entityMap.TryGetValue(action.EntityId, out var entity))
        {
            yield return new ValidationError.EntityNotFound(action.EntityId);
            yield break;
        }
        
        var (part, _) = entity;
        
        // 验证动作与零件类型的兼容性
        var compatibilityError = ValidateActionCompatibility(action.EntityId, part.PartType, action.PartAction);
        if (compatibilityError.IsSome)
        {
            yield return compatibilityError.IfNone(() => throw new InvalidOperationException());
        }
    }
    
    /// <summary>验证动作与零件类型的兼容性</summary>
    private static Option<ValidationError> ValidateActionCompatibility(
        EntityId entityId,
        PartType partType,
        PartAction action)
    {
        return (partType, action) switch
        {
            // 电机只能执行电机动作
            (PartType.Motor _, PartAction.Motor _) => None,
            (PartType.Motor _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    action,
                    "Motor parts can only execute motor actions")),
            
            // 执行器只能执行执行器动作
            (PartType.Actuator actuatorType, PartAction.Actuator actuatorAction) =>
                ValidateActuatorActionCompatibility(entityId, actuatorType.Type, actuatorAction.Action),
            (PartType.Actuator _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    action,
                    "Actuator parts can only execute actuator actions")),
            
            // 传感器不能执行动作
            (PartType.Sensor _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    action,
                    "Sensor parts cannot execute actions")),
            
            // 静态零件不能执行动作
            (PartType.Static _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    action,
                    "Static parts cannot execute actions")),
            
            _ => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    action,
                    "Unknown part type or action type"))
        };
    }
    
    /// <summary>验证执行器动作的兼容性</summary>
    private static Option<ValidationError> ValidateActuatorActionCompatibility(
        EntityId entityId,
        ActuatorType actuatorType,
        ActuatorAction action)
    {
        return (actuatorType, action) switch
        {
            // 气缸：只能伸出或缩回
            (ActuatorType.Cylinder _, ActuatorAction.Extend _) => None,
            (ActuatorType.Cylinder _, ActuatorAction.Retract _) => None,
            (ActuatorType.Cylinder _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    new PartAction.Actuator(action),
                    "Cylinder can only execute Extend or Retract actions")),
            
            // 夹爪：只能闭合或松开
            (ActuatorType.Gripper _, ActuatorAction.Close _) => None,
            (ActuatorType.Gripper _, ActuatorAction.Open _) => None,
            (ActuatorType.Gripper _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    new PartAction.Actuator(action),
                    "Gripper can only execute Close or Open actions")),
            
            // 吸气装置：只能吸气或常规
            (ActuatorType.Suction _, ActuatorAction.Suction _) => None,
            (ActuatorType.Suction _, ActuatorAction.Normal _) => None,
            (ActuatorType.Suction _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    new PartAction.Actuator(action),
                    "Suction can only execute Suction or Normal actions")),
            
            // 指示灯：只能开或关
            (ActuatorType.Indicator _, ActuatorAction.On _) => None,
            (ActuatorType.Indicator _, ActuatorAction.Off _) => None,
            (ActuatorType.Indicator _, _) => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    new PartAction.Actuator(action),
                    "Indicator can only execute On or Off actions")),
            
            _ => Some<ValidationError>(
                new ValidationError.IncompatibleAction(
                    entityId,
                    new PartAction.Actuator(action),
                    "Unknown actuator type or action"))
        };
    }
    
    /// <summary>验证 If 语句</summary>
    private static IEnumerable<ValidationError> ValidateIfStatement(
        Ast.Statement.If ifStmt,
        Dictionary<EntityId, (Part Part, PartConfig Config)> entityMap)
    {
        // 验证条件
        foreach (var error in ValidateCondition(ifStmt.Condition, entityMap))
        {
            yield return error;
        }
        
        // 验证 then 分支
        foreach (var error in ValidateStatement(ifStmt.ThenBranch, entityMap))
        {
            yield return error;
        }
        
        // 验证 else 分支（如果存在）
        if (ifStmt.ElseBranch.IsSome)
        {
            foreach (var error in ValidateStatement(
                ifStmt.ElseBranch.IfNone(() => throw new InvalidOperationException()),
                entityMap))
            {
                yield return error;
            }
        }
    }
    
    /// <summary>验证条件表达式</summary>
    private static IEnumerable<ValidationError> ValidateCondition(
        Ast.Condition condition,
        Dictionary<EntityId, (Part Part, PartConfig Config)> entityMap)
    {
        return condition switch
        {
            Ast.Condition.SensorState sensorState =>
                ValidateSensorReference(sensorState.SensorId, entityMap),
            
            Ast.Condition.StateSensor stateSensor =>
                ValidateStateSensorReference(stateSensor.SensorId, entityMap),
            
            Ast.Condition.SensorValue sensorValue =>
                ValidateSensorReference(sensorValue.SensorId, entityMap),
            
            Ast.Condition.And and =>
                ValidateCondition(and.Left, entityMap)
                    .Concat(ValidateCondition(and.Right, entityMap)),
            
            Ast.Condition.Or or =>
                ValidateCondition(or.Left, entityMap)
                    .Concat(ValidateCondition(or.Right, entityMap)),
            
            Ast.Condition.Not not =>
                ValidateCondition(not.Inner, entityMap),
            
            _ => Seq1(new ValidationError.InvalidCondition("Unknown condition type"))
        };
    }
    
    /// <summary>验证传感器引用</summary>
    /// <remarks>验证：需求 8.3, 9.2</remarks>
    private static IEnumerable<ValidationError> ValidateSensorReference(
        EntityId sensorId,
        Dictionary<EntityId, (Part Part, PartConfig Config)> entityMap)
    {
        // 检查实体是否存在
        if (!entityMap.TryGetValue(sensorId, out var entity))
        {
            yield return new ValidationError.InvalidSensorReference(
                sensorId,
                "Sensor entity not found in machine");
            yield break;
        }
        
        var (part, _) = entity;
        
        // 检查实体是否为传感器类型
        if (part.PartType is not PartType.Sensor)
        {
            yield return new ValidationError.InvalidSensorReference(
                sensorId,
                $"Entity is not a sensor, but a {part.PartType.GetType().Name}");
        }
    }
    
    /// <summary>验证状态传感器引用</summary>
    /// <remarks>
    /// 验证状态传感器引用是否有效。状态传感器是执行器（如气缸）的状态反馈传感器。
    /// 注意：此验证需要额外的状态传感器映射表，当前实现仅做基本检查。
    /// 完整实现需要在机器配置中维护 StateSensorId -> (EntityId, SensorPort) 的映射。
    /// 验证：需求 28.8
    /// </remarks>
    private static IEnumerable<ValidationError> ValidateStateSensorReference(
        Ast.StateSensorId sensorId,
        Dictionary<EntityId, (Part Part, PartConfig Config)> entityMap)
    {
        // TODO: 完整实现需要状态传感器映射表
        // 当前仅做基本检查：确保至少有一个执行器配置了状态传感器
        
        var hasActuatorWithStateSensor = entityMap.Values.Any(entity =>
        {
            var (part, config) = entity;
            return part.PartType is PartType.Actuator && HasStateSensorConfig(config);
        });
        
        if (!hasActuatorWithStateSensor)
        {
            yield return new ValidationError.InvalidStateSensorReference(
                sensorId,
                "No actuators with state sensor configuration found in machine");
        }
        
        // 注意：完整验证需要检查特定的 StateSensorId 是否对应某个执行器的配置
        // 这需要在配置阶段建立 StateSensorId 到执行器的映射关系
    }
    
    /// <summary>检查配置是否包含状态传感器</summary>
    private static bool HasStateSensorConfig(PartConfig config)
    {
        return config switch
        {
            PartConfig.Actuator actuatorConfig => actuatorConfig.Config.StateSensorPorts.IsSome,
            _ => false
        };
    }
}
