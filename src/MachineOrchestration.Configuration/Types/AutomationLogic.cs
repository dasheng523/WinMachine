using MachineOrchestration.Dsl.Ast;

namespace MachineOrchestration.Configuration.Types;

/// <summary>自动化逻辑</summary>
/// <remarks>
/// 包含逻辑 ID、名称和 AST。
/// 验证：需求 14.1-14.5
/// </remarks>
public sealed record AutomationLogic(
    LogicId Id,
    string Name,
    Ast Ast);

/// <summary>逻辑 ID（newtype 模式）</summary>
public readonly record struct LogicId(Guid Value)
{
    public static LogicId New() => new(Guid.NewGuid());
}
