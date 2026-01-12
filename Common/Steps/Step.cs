using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Common.Steps;

/// <summary>
/// 当 Step 失败时，人工/上位机可做的决策。
/// </summary>
public enum StepDecision
{
    Retry,
    Skip,
    Abort
}

/// <summary>
/// Step 执行的“结果状态”。
/// - Succeeded：成功完成。
/// - Skipped：人为选择跳过（一般用于非关键步骤/可降级步骤）。
/// - Aborted：人为选择中止（通常意味着需要人工介入）。
/// - Failed：失败（通常表示没有走到可交互决策，或者决策不被允许）。
/// </summary>
public enum StepStatus
{
    Succeeded,
    Skipped,
    Aborted,
    Failed
}

/// <summary>
/// 定义某一步骤在失败时允许的交互能力。
/// 例：某些硬件动作“不能重试”（可能会造成叠料/撞机/不安全），则 CanRetry=false。
/// </summary>
public sealed record StepOnError(bool CanRetry = true, bool CanSkip = true)
{
    public static StepOnError Default { get; } = new();
    public static StepOnError NoRetry { get; } = new(CanRetry: false, CanSkip: true);
    public static StepOnError NoSkip { get; } = new(CanRetry: true, CanSkip: false);
    public static StepOnError NoRetryNoSkip { get; } = new(CanRetry: false, CanSkip: false);
}

/// <summary>
/// 一次失败信息（用于给 UI/人工决策提供上下文）。
/// </summary>
public sealed record StepFailure(
    string Name,
    Error Error,
    int Attempt,
    StepOnError OnError);

/// <summary>
/// 决策提供者：通常由 UI 实现。
/// 注意：并不是每一步都允许 Retry/Skip；请参考 StepFailure.OnError。
/// </summary>
public interface IStepDecisionProvider
{
    IObservable<StepDecision> Decide(StepFailure failure);
}

public sealed record StepContext(IStepDecisionProvider Decisions)
{
    public static StepContext Create(IStepDecisionProvider decisions) => new(decisions);
}

public sealed record StepNode(string Name, IReadOnlyList<StepNode> Children)
{
    public static StepNode Leaf(string name) => new(name, System.Array.Empty<StepNode>());
    public static StepNode Seq(StepNode left, StepNode right) => new("(seq)", new[] { left, right });
    public static StepNode Named(string name, StepNode inner) => new(name, new[] { inner });
}

public readonly record struct StepOutcome<A>(StepStatus Status, Fin<A> Value)
{
    public static StepOutcome<A> Succeeded(A a) => new(StepStatus.Succeeded, FinSucc(a));
    public static StepOutcome<A> Skipped(A a, Error _reason) => new(StepStatus.Skipped, FinSucc(a));
    public static StepOutcome<A> Aborted(Error e) => new(StepStatus.Aborted, FinFail<A>(e));
    public static StepOutcome<A> Failed(Error e) => new(StepStatus.Failed, FinFail<A>(e));
}

/// <summary>
/// Step Monad (Rx):
/// - 以 Linq(do-notation) 组合多个步骤。
/// - 每个步骤失败时，可通过 IStepDecisionProvider 人工选择 Retry/Skip/Abort。
/// 说明：这是用于 DSL 外观定型的最小实现；后续可补齐更完整的 trace / 子步骤结果汇总。
/// </summary>
public readonly record struct Step<A>(StepNode Node, Func<StepContext, IObservable<StepOutcome<A>>> Run)
{
    /// <summary>
    /// Functor map。
    /// </summary>
    public Step<B> Select<B>(Func<A, B> f)
    {
        var node = Node;
        var run = Run;

        return new Step<B>(
            node,
            ctx => run(ctx).Select(o =>
                o.Value.Match(
                    Succ: a => o.Status == StepStatus.Skipped
                        ? new StepOutcome<B>(StepStatus.Skipped, FinSucc(f(a)))
                        : new StepOutcome<B>(o.Status, FinSucc(f(a))),
                    Fail: e => new StepOutcome<B>(o.Status, FinFail<B>(e)))));
    }

    /// <summary>
    /// Monad bind。
    /// 说明：当前 Node 的组合为了“外观定型”，使用 bind(default) 推断右侧结构。
    /// 因此建议 bind 不依赖输入值来决定“结构”，只依赖输入值来决定“执行行为”。
    /// 后续如果你希望完全描述化（先建 AST，再解释执行），我们可以把结构构建与执行彻底解耦。
    /// </summary>
    public Step<B> SelectMany<B>(Func<A, Step<B>> bind)
    {
        var leftNode = Node;
        var leftRun = Run;

        // NOTE: Node 组合时使用 default(A) 推断子节点；用于“外观定型”的 DSL，推荐 bind 不依赖 A。
        // 若 bind 依赖 A 来构造不同结构，请改用显式 Then/Named 组合（后续可扩展为延迟描述）。
        var rightNode = bind(default!).Node;
        var node = StepNode.Seq(leftNode, rightNode);

        return new Step<B>(
            node,
            ctx => leftRun(ctx).SelectMany(o1 =>
                o1.Value.Match(
                    Succ: a => bind(a).Run(ctx),
                    Fail: e => Observable.Return(new StepOutcome<B>(o1.Status, FinFail<B>(e))))));
    }

    public Step<C> SelectMany<B, C>(Func<A, Step<B>> bind, Func<A, B, C> project) =>
        SelectMany(a => bind(a).Select(b => project(a, b)));
}

public static class Step
{
    /// <summary>
    /// 纯值：不产生副作用。
    /// </summary>
    public static Step<A> Pure<A>(A value) =>
        new(
            new StepNode("(pure)", System.Array.Empty<StepNode>()),
            _ => Observable.Return(StepOutcome<A>.Succeeded(value)));

    public static Step<LUnit> Unit => Pure(unit);

    /// <summary>
    /// 给一个 Step 命名，使之成为一个“可包含子步骤”的组。
    /// </summary>
    public static Step<A> Named<A>(string name, Step<A> inner) =>
        new(StepNode.Named(name, inner.Node), inner.Run);

    /// <summary>
    /// 一个有副作用的原子步骤。
    /// - action：返回 Fin<Unit>，Fail 表示“该步骤失败”。
    /// - onError：定义失败时允许 Retry/Skip。
    /// </summary>
    public static Step<LUnit> Effect(string name, Func<StepContext, IObservable<Fin<LUnit>>> action) =>
        Effect(name, action, StepOnError.Default);

    public static Step<LUnit> Effect(
        string name,
        Func<StepContext, IObservable<Fin<LUnit>>> action,
        StepOnError onError) =>
        new(
            StepNode.Leaf(name),
            ctx => ExecuteWithDecision(name, ctx, action, onError));

    private static IObservable<StepOutcome<LUnit>> ExecuteWithDecision(
        string name,
        StepContext ctx,
        Func<StepContext, IObservable<Fin<LUnit>>> action,
        StepOnError onError)
    {
        IObservable<Fin<LUnit>> SafeCall()
        {
            try
            {
                return action(ctx)
                    .Catch<Fin<LUnit>, Exception>(ex =>
                        Observable.Return(FinFail<LUnit>(Error.New(ex))));
            }
            catch (Exception ex)
            {
                return Observable.Return(FinFail<LUnit>(Error.New(ex)));
            }
        }

        IObservable<StepOutcome<LUnit>> Attempt(int attempt) =>
            SafeCall().SelectMany(fin =>
                fin.Match(
                    Succ: _ => Observable.Return(StepOutcome<LUnit>.Succeeded(unit)),
                    Fail: e =>
                    {
                        var failure = new StepFailure(name, e, attempt, onError);
                        return ctx.Decisions.Decide(failure).SelectMany(d =>
                            d switch
                            {
                                StepDecision.Retry when onError.CanRetry => Attempt(attempt + 1),
                                StepDecision.Retry => Observable.Return(
                                    StepOutcome<LUnit>.Aborted(Error.New($"步骤 '{name}' 不允许重试"))),

                                StepDecision.Skip when onError.CanSkip => Observable.Return(StepOutcome<LUnit>.Skipped(unit, e)),
                                StepDecision.Skip => Observable.Return(
                                    StepOutcome<LUnit>.Aborted(Error.New($"步骤 '{name}' 不允许跳过"))),

                                StepDecision.Abort => Observable.Return(StepOutcome<LUnit>.Aborted(e)),
                                _ => Observable.Return(StepOutcome<LUnit>.Failed(e))
                            });
                    }));

        return Attempt(attempt: 1);
    }
}
