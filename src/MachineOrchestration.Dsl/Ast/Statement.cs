using System;
using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Dsl.Ast;

/// <summary>DSL 语句（和类型 - Sum Type）</summary>
/// <remarks>
/// 实现 Action, Wait, WaitUntil, Sequence, Parallel, Loop, If 变体。
/// 使用 sealed record 模式实现和类型。
/// 验证：需求 8.1-8.4
/// </remarks>
public abstract record Statement
{
    /// <summary>动作执行</summary>
    /// <remarks>执行指定实体的动作</remarks>
    public sealed record Action(
        EntityId EntityId,
        PartAction PartAction) : Statement;
    
    /// <summary>等待指定时间</summary>
    /// <remarks>暂停执行指定的时间长度</remarks>
    public sealed record Wait(TimeSpan Duration) : Statement;
    
    /// <summary>等待条件满足</summary>
    /// <remarks>暂停执行直到条件为真</remarks>
    public sealed record WaitUntil(Condition Condition) : Statement;
    
    /// <summary>顺序执行</summary>
    /// <remarks>按顺序执行一系列语句</remarks>
    public sealed record Sequence(Seq<Statement> Statements) : Statement;
    
    /// <summary>并行执行</summary>
    /// <remarks>同时执行多个语句</remarks>
    public sealed record Parallel(Seq<Statement> Statements) : Statement;
    
    /// <summary>循环执行</summary>
    /// <remarks>
    /// 重复执行语句体。如果 Count 为 None，则无限循环。
    /// </remarks>
    public sealed record Loop(
        Option<uint> Count,
        Statement Body) : Statement;
    
    /// <summary>条件分支</summary>
    /// <remarks>
    /// 根据条件执行不同的分支。如果条件为真，执行 ThenBranch；
    /// 否则，如果 ElseBranch 存在，执行 ElseBranch。
    /// </remarks>
    public sealed record If(
        Condition Condition,
        Statement ThenBranch,
        Option<Statement> ElseBranch) : Statement;
    
    // 私有构造函数确保只能通过上述变体创建实例
    private Statement() { }
}
