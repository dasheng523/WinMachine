using System;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Dsl.Ast;

/// <summary>状态传感器 ID（newtype 模式）</summary>
/// <remarks>
/// 用于标识执行器的状态传感器（如气缸的伸出/缩回传感器）。
/// 验证：需求 28.8
/// </remarks>
public readonly record struct StateSensorId(Guid Value)
{
    /// <summary>创建新的状态传感器 ID</summary>
    public static StateSensorId NewId() => new(Guid.NewGuid());
}

/// <summary>比较运算符</summary>
/// <remarks>
/// 用于传感器值比较的运算符。
/// 验证：需求 8.1-8.4
/// </remarks>
public enum ComparisonOp
{
    /// <summary>等于</summary>
    Equal,
    
    /// <summary>不等于</summary>
    NotEqual,
    
    /// <summary>大于</summary>
    Greater,
    
    /// <summary>大于或等于</summary>
    GreaterOrEqual,
    
    /// <summary>小于</summary>
    Less,
    
    /// <summary>小于或等于</summary>
    LessOrEqual
}

/// <summary>DSL 条件表达式（和类型 - Sum Type）</summary>
/// <remarks>
/// 实现 SensorState, StateSensor, SensorValue, And, Or, Not 变体。
/// 使用 sealed record 模式实现和类型。
/// 验证：需求 8.1-8.4, 28.8
/// </remarks>
public abstract record Condition
{
    /// <summary>传感器状态检查</summary>
    /// <remarks>
    /// 检查传感器（输入类型零件）的状态是否与期望值匹配。
    /// </remarks>
    public sealed record SensorState(
        EntityId SensorId,
        bool Expected) : Condition;
    
    /// <summary>状态传感器检查</summary>
    /// <remarks>
    /// 检查执行器的状态传感器（如气缸的伸出/缩回传感器）是否与期望值匹配。
    /// 用于等待执行器到位的条件判断。
    /// 验证：需求 28.8
    /// </remarks>
    public sealed record StateSensor(
        StateSensorId SensorId,
        bool Expected) : Condition;
    
    /// <summary>传感器值比较</summary>
    /// <remarks>
    /// 将传感器读数与指定值进行比较。
    /// </remarks>
    public sealed record SensorValue(
        EntityId SensorId,
        ComparisonOp Operator,
        float Value) : Condition;
    
    /// <summary>逻辑与</summary>
    /// <remarks>
    /// 两个条件都为真时，结果为真。
    /// </remarks>
    public sealed record And(Condition Left, Condition Right) : Condition;
    
    /// <summary>逻辑或</summary>
    /// <remarks>
    /// 至少一个条件为真时，结果为真。
    /// </remarks>
    public sealed record Or(Condition Left, Condition Right) : Condition;
    
    /// <summary>逻辑非</summary>
    /// <remarks>
    /// 条件为假时，结果为真；条件为真时，结果为假。
    /// </remarks>
    public sealed record Not(Condition Inner) : Condition;
    
    // 私有构造函数确保只能通过上述变体创建实例
    private Condition() { }
}
