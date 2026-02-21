using System;
using MachineOrchestration.Dsl.Ast;

namespace MachineOrchestration.Automation.Types;

/// <summary>逻辑 ID（newtype 模式）</summary>
/// <remarks>
/// 使用 newtype 模式确保类型安全，避免混淆不同类型的 ID。
/// 验证：需求 14.1
/// </remarks>
public readonly record struct LogicId(Guid Value)
{
    /// <summary>创建新的逻辑 ID</summary>
    public static LogicId NewId() => new(Guid.NewGuid());
    
    /// <summary>从字符串解析逻辑 ID</summary>
    public static LogicId Parse(string value) => new(Guid.Parse(value));
    
    /// <summary>尝试从字符串解析逻辑 ID</summary>
    public static bool TryParse(string value, out LogicId logicId)
    {
        if (Guid.TryParse(value, out var guid))
        {
            logicId = new LogicId(guid);
            return true;
        }
        logicId = default;
        return false;
    }
    
    public override string ToString() => Value.ToString();
}

/// <summary>自动化逻辑记录类型</summary>
/// <remarks>
/// 包含逻辑 ID、名称和 AST。
/// 这是一个不可变的记录类型，表示一个完整的自动化逻辑定义。
/// 验证：需求 14.1-14.5
/// </remarks>
public sealed record AutomationLogic(
    LogicId Id,
    string Name,
    Ast Ast)
{
    /// <summary>
    /// 创建新的自动化逻辑
    /// </summary>
    /// <param name="name">逻辑名称</param>
    /// <param name="ast">抽象语法树</param>
    /// <returns>新的自动化逻辑实例</returns>
    public static AutomationLogic Create(string name, Ast ast) =>
        new(LogicId.NewId(), name, ast);
    
    /// <summary>
    /// 使用指定 ID 创建自动化逻辑
    /// </summary>
    /// <param name="id">逻辑 ID</param>
    /// <param name="name">逻辑名称</param>
    /// <param name="ast">抽象语法树</param>
    /// <returns>新的自动化逻辑实例</returns>
    public static AutomationLogic CreateWithId(LogicId id, string name, Ast ast) =>
        new(id, name, ast);
    
    /// <summary>
    /// 更新逻辑名称
    /// </summary>
    /// <param name="newName">新名称</param>
    /// <returns>更新后的自动化逻辑</returns>
    public AutomationLogic WithName(string newName) => this with { Name = newName };
    
    /// <summary>
    /// 更新 AST
    /// </summary>
    /// <param name="newAst">新的抽象语法树</param>
    /// <returns>更新后的自动化逻辑</returns>
    public AutomationLogic WithAst(Ast newAst) => this with { Ast = newAst };
}
