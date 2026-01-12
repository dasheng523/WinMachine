using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Common.Steps;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace WinMachine.Tests;

public sealed class StepDslShapeTests
{
    [Fact]
    public void StepDsl_ShouldDescribeLoadingFlowShape()
    {
        // DSL 外观（上料过程）：
        // X/Y 到物料位 -> Z 下 -> 吸气 -> Z 上安全位 -> 压力达标判断
        var load = Step.Named(
            "上料",
            from _1 in Step.Effect("X和Y轴到达物料位置", _ => Observable.Return(FinSucc(unit)))
            from _2 in Step.Effect("Z轴下达物料位置", _ => Observable.Return(FinSucc(unit)))
            from _3 in Step.Effect("吸笔吸气", _ => Observable.Return(FinSucc(unit)))
            from _4 in Step.Effect("Z轴上移到安全位置", _ => Observable.Return(FinSucc(unit)))
            from _5 in Step.Effect("判断压力传感器是否达标", _ => Observable.Return(FinSucc(unit)))
            select unit);

        var leafNames = FlattenLeafNames(load.Node).ToArray();

        load.Node.Name.Should().Be("上料");
        leafNames.Should().Equal(
            "X和Y轴到达物料位置",
            "Z轴下达物料位置",
            "吸笔吸气",
            "Z轴上移到安全位置",
            "判断压力传感器是否达标");
    }

    [Fact]
    public void StepDsl_ShouldSupportManualRetry()
    {
        var attempts = 0;
        var step = Step.Effect(
            "判断压力传感器是否达标",
            _ => Observable.Defer(() =>
            {
                attempts++;
                return attempts == 1
                    ? Observable.Return(FinFail<LUnit>(Error.New("压力不足")))
                    : Observable.Return(FinSucc(unit));
            }));

        var ctx = StepContext.Create(new ScriptedDecisionProvider(StepDecision.Retry));
        var outcome = step.Run(ctx).Wait();

        attempts.Should().Be(2);
        outcome.Status.Should().Be(StepStatus.Succeeded);
        outcome.Value.IsSucc.Should().BeTrue();
    }

    [Fact]
    public void StepDsl_ShouldSupportManualSkip()
    {
        var step = Step.Effect(
            "判断压力传感器是否达标",
            _ => Observable.Return(FinFail<LUnit>(Error.New("压力不足"))));

        var ctx = StepContext.Create(new ScriptedDecisionProvider(StepDecision.Skip));
        var outcome = step.Run(ctx).Wait();

        outcome.Status.Should().Be(StepStatus.Skipped);
        outcome.Value.IsSucc.Should().BeTrue("Skip 语义：继续流程（Unit）");
    }

    [Fact]
    public void StepDsl_ShouldRejectRetryWhenStepNotRetryable()
    {
        var step = Step.Effect(
            "吸笔吸气",
            _ => Observable.Return(FinFail<LUnit>(Error.New("真空异常"))),
            StepOnError.NoRetry);

        var ctx = StepContext.Create(new ScriptedDecisionProvider(StepDecision.Retry));
        var outcome = step.Run(ctx).Wait();

        outcome.Status.Should().Be(StepStatus.Aborted);
        outcome.Value.IsFail.Should().BeTrue();
    }

    private static IEnumerable<string> FlattenLeafNames(StepNode node)
    {
        if (node.Children.Count == 0)
        {
            yield return node.Name;
            yield break;
        }

        foreach (var c in node.Children)
        {
            foreach (var x in FlattenLeafNames(c))
            {
                yield return x;
            }
        }
    }

    private sealed class ScriptedDecisionProvider : IStepDecisionProvider
    {
        private readonly Queue<StepDecision> _script;

        public ScriptedDecisionProvider(params StepDecision[] script) =>
            _script = new Queue<StepDecision>(script);

        public IObservable<StepDecision> Decide(StepFailure failure)
        {
            if (_script.Count == 0)
            {
                // 外观定型阶段：如果脚本没写完，默认 Abort，避免无限重试。
                return Observable.Return(StepDecision.Abort);
            }

            return Observable.Return(_script.Dequeue());
        }
    }
}
