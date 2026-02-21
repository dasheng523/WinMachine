using LanguageExt;
using AstType = MachineOrchestration.Dsl.Ast.Ast;

namespace MachineOrchestration.Dsl.Interpreter;

/// <summary>DSL 解释器接口（纯函数）</summary>
/// <remarks>
/// 定义 DSL 解释器的纯函数接口。所有方法都是纯函数，不包含副作用。
/// 状态转换通过返回新的 ExecutionState 实现，而不是修改现有状态。
/// 验证：需求 15.1-15.6
/// </remarks>
public interface IDslInterpreter
{
    /// <summary>执行一步（纯函数）</summary>
    /// <param name="state">当前执行状态</param>
    /// <param name="ast">要执行的 AST</param>
    /// <returns>
    /// 成功时返回新的执行状态，失败时返回执行错误。
    /// 此方法是纯函数，不修改输入状态。
    /// </returns>
    /// <remarks>
    /// 根据当前程序计数器执行一条语句，并返回更新后的状态。
    /// 对于不同的语句类型：
    /// - Action: 更新机器状态以反映动作
    /// - Wait: 设置等待时间
    /// - WaitUntil: 评估条件，如果为真则继续
    /// - Sequence: 按顺序执行语句
    /// - Parallel: 标记并行执行（实际并行由执行器处理）
    /// - Loop: 处理循环迭代
    /// - If: 评估条件并选择分支
    /// 验证：需求 15.1-15.6
    /// </remarks>
    Either<ExecutionError, ExecutionState> Step(
        ExecutionState state,
        AstType ast);
    
    /// <summary>检查执行是否完成</summary>
    /// <param name="state">当前执行状态</param>
    /// <returns>如果执行完成返回 true，否则返回 false</returns>
    /// <remarks>
    /// 纯函数，仅检查状态的 IsComplete 标志。
    /// 验证：需求 15.1-15.6
    /// </remarks>
    bool IsComplete(ExecutionState state);
}

