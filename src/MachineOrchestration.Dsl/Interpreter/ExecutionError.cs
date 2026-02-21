using System;
using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Dsl.Interpreter;

/// <summary>执行错误（和类型）</summary>
/// <remarks>
/// 表示 DSL 执行期间可能发生的错误。
/// 验证：需求 24.1-24.6
/// </remarks>
public abstract record ExecutionError
{
    /// <summary>硬件错误</summary>
    public sealed record HardwareError(
        string Message,
        Option<EntityId> EntityId) : ExecutionError;
    
    /// <summary>超时错误</summary>
    public sealed record Timeout(
        string Message,
        TimeSpan Duration) : ExecutionError;
    
    /// <summary>无效状态转换</summary>
    public sealed record InvalidStateTransition(
        string Message,
        string CurrentState,
        string AttemptedTransition) : ExecutionError;
    
    /// <summary>传感器错误</summary>
    public sealed record SensorError(
        string Message,
        EntityId SensorId) : ExecutionError;
    
    /// <summary>实体未找到</summary>
    public sealed record EntityNotFound(
        EntityId EntityId) : ExecutionError;
    
    /// <summary>条件评估错误</summary>
    public sealed record ConditionEvaluationError(
        string Message) : ExecutionError;
    
    /// <summary>未知错误</summary>
    public sealed record Unknown(string Message) : ExecutionError;
    
    private ExecutionError() { }
    
    /// <summary>获取错误消息</summary>
    public string GetMessage() => this switch
    {
        HardwareError e => $"Hardware error: {e.Message}",
        Timeout e => $"Timeout after {e.Duration}: {e.Message}",
        InvalidStateTransition e => $"Invalid state transition from '{e.CurrentState}' attempting '{e.AttemptedTransition}': {e.Message}",
        SensorError e => $"Sensor error for {e.SensorId}: {e.Message}",
        EntityNotFound e => $"Entity not found: {e.EntityId}",
        ConditionEvaluationError e => $"Condition evaluation error: {e.Message}",
        Unknown e => $"Unknown error: {e.Message}",
        _ => "Unhandled error"
    };
}

