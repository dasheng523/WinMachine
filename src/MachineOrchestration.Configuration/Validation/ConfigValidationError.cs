using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Configuration.Validation;

/// <summary>配置验证错误（和类型 - Sum Type）</summary>
/// <remarks>
/// 表示配置验证过程中可能出现的各种错误。
/// 验证：需求 11.9-11.12, 12.2-12.4
/// </remarks>
public abstract record ConfigValidationError
{
    /// <summary>缺少必需字段</summary>
    public sealed record MissingField(string FieldName) : ConfigValidationError;
    
    /// <summary>字段值无效</summary>
    public sealed record InvalidValue(string Field, string Reason) : ConfigValidationError;
    
    /// <summary>缺少传感器端口配置</summary>
    /// <remarks>
    /// 配置了状态传感器但未指定传感器端口。
    /// 验证：需求 11.9-11.10
    /// </remarks>
    public sealed record MissingSensorPort(EntityId EntityId, string SensorType) : ConfigValidationError;
    
    /// <summary>控制板配置不兼容</summary>
    /// <remarks>
    /// 控制板参数与所选控制板类型不兼容。
    /// 验证：需求 12.2-12.4
    /// </remarks>
    public sealed record IncompatibleBoardConfig(string Message) : ConfigValidationError;
    
    /// <summary>电机配置无效</summary>
    /// <remarks>电机配置参数无效（如速度超出范围）</remarks>
    public sealed record InvalidMotorConfig(EntityId MotorId, string Reason) : ConfigValidationError;
    
    /// <summary>执行器传感器配置无效</summary>
    /// <remarks>执行器的状态传感器配置无效</remarks>
    public sealed record InvalidActuatorSensorConfig(EntityId ActuatorId, string Reason) : ConfigValidationError;
    
    /// <summary>端口冲突</summary>
    /// <remarks>多个零件配置了相同的端口</remarks>
    public sealed record PortConflict(ushort Port, Seq<EntityId> ConflictingEntities) : ConfigValidationError;
    
    /// <summary>多个验证错误</summary>
    /// <remarks>
    /// 收集多个验证错误以便一次性报告所有问题。
    /// 验证：需求 11.11-11.12
    /// </remarks>
    public sealed record Multiple(Seq<ConfigValidationError> Errors) : ConfigValidationError;
    
    // 私有构造函数确保只能通过上述变体创建实例
    private ConfigValidationError() { }
    
    /// <summary>获取错误的描述性消息</summary>
    public string GetMessage() => this switch
    {
        MissingField(var field) => $"缺少必需字段: {field}",
        InvalidValue(var field, var reason) => $"字段 '{field}' 的值无效: {reason}",
        MissingSensorPort(var entityId, var sensorType) => 
            $"实体 {entityId.Value} 配置了 {sensorType} 但未指定传感器端口",
        IncompatibleBoardConfig(var message) => $"控制板配置不兼容: {message}",
        InvalidMotorConfig(var motorId, var reason) => 
            $"电机 {motorId.Value} 配置无效: {reason}",
        InvalidActuatorSensorConfig(var actuatorId, var reason) => 
            $"执行器 {actuatorId.Value} 传感器配置无效: {reason}",
        PortConflict(var port, var entities) => 
            $"端口 {port} 冲突，被以下实体使用: {string.Join(", ", entities.Map(e => e.Value))}",
        Multiple(var errors) => 
            $"发现 {errors.Count} 个验证错误:\n" + 
            string.Join("\n", errors.Map(e => $"  - {e.GetMessage()}")),
        _ => "未知验证错误"
    };
}
