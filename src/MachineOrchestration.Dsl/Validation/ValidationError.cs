using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Dsl.Validation;

/// <summary>DSL 验证错误（和类型 - Sum Type）</summary>
/// <remarks>
/// 表示 DSL 语义验证过程中可能出现的各种错误。
/// 验证：需求 8.3, 9.2
/// </remarks>
public abstract record ValidationError
{
    /// <summary>实体 ID 不存在</summary>
    /// <remarks>引用的实体 ID 在机器中不存在</remarks>
    public sealed record EntityNotFound(EntityId EntityId) : ValidationError;
    
    /// <summary>传感器引用无效</summary>
    /// <remarks>引用的传感器不存在或类型不匹配</remarks>
    public sealed record InvalidSensorReference(EntityId SensorId, string Reason) : ValidationError;
    
    /// <summary>状态传感器引用无效</summary>
    /// <remarks>
    /// 引用的状态传感器不存在或未配置。
    /// 验证：需求 28.8
    /// </remarks>
    public sealed record InvalidStateSensorReference(Ast.StateSensorId SensorId, string Reason) : ValidationError;
    
    /// <summary>动作与零件类型不兼容</summary>
    /// <remarks>尝试对零件执行不兼容的动作（如对传感器执行电机动作）</remarks>
    public sealed record IncompatibleAction(EntityId EntityId, PartAction Action, string Reason) : ValidationError;
    
    /// <summary>条件表达式无效</summary>
    /// <remarks>条件表达式引用的传感器或状态传感器无效</remarks>
    public sealed record InvalidCondition(string Reason) : ValidationError;
    
    /// <summary>多个验证错误</summary>
    /// <remarks>收集多个验证错误以便一次性报告所有问题</remarks>
    public sealed record Multiple(Seq<ValidationError> Errors) : ValidationError;
    
    // 私有构造函数确保只能通过上述变体创建实例
    private ValidationError() { }
}
