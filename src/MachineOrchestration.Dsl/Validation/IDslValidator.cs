using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Dsl.Validation;

/// <summary>DSL 验证器接口（纯函数）</summary>
/// <remarks>
/// 验证 DSL 抽象语法树的语义正确性。
/// 由于使用 C# 代码直接构建 AST，无需解析，只需验证语义。
/// 验证：需求 8.3, 9.2
/// </remarks>
public interface IDslValidator
{
    /// <summary>验证 AST 的语义正确性</summary>
    /// <param name="ast">要验证的抽象语法树</param>
    /// <param name="machine">机器定义，用于验证实体 ID 和传感器引用</param>
    /// <returns>
    /// 如果验证成功，返回 Right(Unit)；
    /// 如果验证失败，返回 Left(ValidationError)，包含所有验证错误
    /// </returns>
    /// <remarks>
    /// 验证内容包括：
    /// - 实体 ID 是否存在于机器中
    /// - 传感器引用是否有效
    /// - 动作是否与零件类型兼容
    /// - 状态传感器引用是否有效（针对执行器）
    /// 验证：需求 8.3, 9.2, 28.8
    /// </remarks>
    Either<ValidationError, Unit> Validate(Ast.Ast ast, ComposableEntity machine);
}
