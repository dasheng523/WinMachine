using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MachineOrchestration.Dsl.Ast;
using MachineOrchestration.Dsl.Interpreter;

namespace MachineOrchestration.Automation.Executor;

/// <summary>
/// DSL 执行器接口（副作用边界）
/// 负责执行 DSL 程序并管理副作用（控制板命令、状态流等）
/// </summary>
/// <remarks>
/// 验证：需求 15.1-15.6
/// </remarks>
public interface IDslExecutor
{
    /// <summary>
    /// 执行自动化逻辑
    /// </summary>
    /// <param name="ast">要执行的 AST</param>
    /// <param name="initialState">初始执行状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果，成功返回 Unit，失败返回错误</returns>
    Task<Either<ExecutionError, Unit>> Execute(
        Ast ast,
        ExecutionState initialState,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 停止当前执行
    /// </summary>
    /// <returns>停止结果，成功返回 Unit，失败返回错误</returns>
    Task<Either<ExecutionError, Unit>> Stop();
    
    /// <summary>
    /// 执行状态流（响应式）
    /// 使用 System.Reactive 暴露执行状态变化
    /// </summary>
    IObservable<ExecutionState> ExecutionStateStream { get; }
    
    /// <summary>
    /// 执行是否正在运行
    /// </summary>
    bool IsRunning { get; }
}
