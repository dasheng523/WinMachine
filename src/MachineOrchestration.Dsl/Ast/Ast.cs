using LanguageExt;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Dsl.Ast;

/// <summary>DSL 抽象语法树</summary>
/// <remarks>
/// 简单包装语句序列,表示完整的 DSL 程序。
/// 验证：需求 8.2
/// </remarks>
public sealed record Ast(Seq<Statement> Statements)
{
    /// <summary>创建空的 AST</summary>
    public static readonly Ast Empty = new(Seq<Statement>());
    
    /// <summary>创建包含单个语句的 AST</summary>
    public static Ast Single(Statement statement) => new(Seq1(statement));
    
    /// <summary>创建包含多个语句的 AST</summary>
    public static Ast Create(params Statement[] statements) => 
        new(Seq(statements));
}
