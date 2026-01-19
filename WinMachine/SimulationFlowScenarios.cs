using System;
using System.Reactive.Linq;
using System.Threading;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Simulation;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace WinMachine;

internal sealed record SimulationFlowScenario(
    string Name,
    Func<CancellationTokenSource, ScenarioRuntime> Create);

internal sealed record ScenarioRuntime(
    FlowContext Context,
    StepDesc Flow,
    Action<FlowContext, System.Reactive.Disposables.CompositeDisposable>? BeforeRun,
    IObservable<SimulationDomainEvent> DomainEvents,
    TransferStationModel? TransferModel);

internal static class SimulationFlowScenarios
{
    public static SimulationFlowScenario[] All =>
    [
        TransferStation_Swap_Left_Then_Right(),
    ];

    private static SimulationFlowScenario TransferStation_Swap_Left_Then_Right() =>
        new(
            "Demo_TransferStation_Swap_Left_Then_Right",
            cts =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b
                        .MapCylinder("Slide", 10)
                        .MapAxis("LeftRotate", 1)
                        .MapAxis("RightRotate", 2)
                        .MapCylinder("LeftLift", 20)
                        .MapCylinder("RightLift", 21)
                        .MapCylinder("LeftGrip", 22)
                        .MapCylinder("RightGrip", 23)
                        .UseSimulator()
                    )
                    .ConfigureCylinder("Slide", c => c.WithDynamicsMs(500))
                    .ConfigureAxis("LeftRotate", a => a.SetSoftLimits(sl => sl.Range(0, 180)))
                    .ConfigureAxis("RightRotate", a => a.SetSoftLimits(sl => sl.Range(0, 180)))
                    .ConfigureCylinder("LeftLift", c => c.WithDynamicsMs(260))
                    .ConfigureCylinder("RightLift", c => c.WithDynamicsMs(260))
                    .ConfigureCylinder("LeftGrip", c => c.WithDynamicsMs(180))
                    .ConfigureCylinder("RightGrip", c => c.WithDynamicsMs(180))
                    .UseSimulator("Main", sim => sim
                        .Axis("LeftRotate", a => a.Travel(0, 180))
                        .Axis("RightRotate", a => a.Travel(0, 180))
                        .Timing(t => t.TickMs = 16)
                    );

                var context = new FlowContext(config, cts.Token);

                var flow =
                    (from _1 in Name("推拉到左侧").Next(Cylinder("Slide").FireAndWait(true))
                     from _2 in Name("左侧夹爪闭合").Next(Cylinder("LeftGrip").FireAndWait(true))
                     from _3 in Name("左侧升起").Next(Cylinder("LeftLift").FireAndWait(true))
                     from _4 in Name("左侧旋转180").Next(Motion("LeftRotate").MoveToAndWait(180))
                     from _5 in Name("左侧下降").Next(Cylinder("LeftLift").FireAndWait(false))
                     from _6 in Name("左侧夹爪张开").Next(Cylinder("LeftGrip").FireAndWait(false))
                     from _7 in Name("推拉到右侧").Next(Cylinder("Slide").FireAndWait(false))
                     from _8 in Name("右侧夹爪闭合").Next(Cylinder("RightGrip").FireAndWait(true))
                     from _9 in Name("右侧升起").Next(Cylinder("RightLift").FireAndWait(true))
                     from _10 in Name("右侧旋转180").Next(Motion("RightRotate").MoveToAndWait(180))
                     from _11 in Name("右侧下降").Next(Cylinder("RightLift").FireAndWait(false))
                     from _12 in Name("右侧夹爪张开").Next(Cylinder("RightGrip").FireAndWait(false))
                     select Unit.Default).Definition;

                var model = TransferStationModel.CreateDemo();
                var events = new System.Reactive.Subjects.Subject<SimulationDomainEvent>();

                Action<FlowContext, System.Reactive.Disposables.CompositeDisposable> beforeRun = (ctx, d) =>
                {
                    // 左侧夹爪：闭合时抓取(扫码座0/1 + 测试座0/1)，张开时释放并互换
                    var leftGrip = ctx.GetDevice<ISimulatorCylinder>("LeftGrip");
                    if (leftGrip != null)
                    {
                        d.Add(leftGrip.StateStream
                            .Where(s => !s.IsMoving)
                            .DistinctUntilChanged(s => s.IsExtended)
                            .Subscribe(s =>
                            {
                                events.OnNext(s.IsExtended
                                    ? new TransferGripEvent(TransferSide.Left, TransferGripAction.Grab)
                                    : new TransferGripEvent(TransferSide.Left, TransferGripAction.ReleaseSwap));
                            }));
                    }

                    var rightGrip = ctx.GetDevice<ISimulatorCylinder>("RightGrip");
                    if (rightGrip != null)
                    {
                        d.Add(rightGrip.StateStream
                            .Where(s => !s.IsMoving)
                            .DistinctUntilChanged(s => s.IsExtended)
                            .Subscribe(s =>
                            {
                                events.OnNext(s.IsExtended
                                    ? new TransferGripEvent(TransferSide.Right, TransferGripAction.Grab)
                                    : new TransferGripEvent(TransferSide.Right, TransferGripAction.ReleaseSwap));
                            }));
                    }
                };

                return new ScenarioRuntime(context, flow, beforeRun, events, model);
            });
}

internal static class StepNextExtensions
{
    public static Step<T> Next<TPrevious, T>(this Step<TPrevious> prev, Step<T> next)
    {
        return prev.SelectMany(_ => next, (_, n) => n);
    }
}

internal static class CylinderConfigExtensions
{
    public static CylinderConfig WithDynamicsMs(this CylinderConfig cfg, int ms)
    {
        cfg.MoveTime = ms;
        return cfg;
    }
}
