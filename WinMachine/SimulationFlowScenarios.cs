using System;
using System.Reactive.Linq;
using System.Threading;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;
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
                // 优化后的 DSL：设备定义直接在板卡内完成，物理映射与参数配置合二为一
                var blueprint = MachineBlueprint.Define("WinMachine_Demo")
                    .AddBoard("Main", 0, b => b
                        .UseSimulator()
                        // 直接定义并映射气缸
                        .AddCylinder(MachineDevices.SlideCyl, 10, 10, c => c.WithDynamics(500))
                        .AddCylinder(MachineDevices.LeftLift, 20, 20, c => c.WithDynamics(260).Vertical())
                        .AddCylinder(MachineDevices.RightLift, 21, 21, c => c.WithDynamics(260).Vertical())
                        .AddCylinder(MachineDevices.LeftGrip, 22, 22, c => c.WithDynamics(180))
                        .AddCylinder(MachineDevices.RightGrip, 23, 23, c => c.WithDynamics(180))
                        // 直接定义并映射轴
                        .AddAxis(MachineDevices.LeftRotate, 1, a => a.WithRange(0, 180))
                        .AddAxis(MachineDevices.RightRotate, 2, a => a.WithRange(0, 180))
                    );

                var config = BlueprintInterpreter.ToConfig(blueprint);
                var context = new FlowContext(config, cts.Token);

                var flow =
                    (from _1 in Name("推拉到左侧").Next(Cylinder(MachineDevices.SlideCyl).FireAndWait(true))
                     from _2 in Name("左侧夹爪闭合").Next(Cylinder(MachineDevices.LeftGrip).FireAndWait(true))
                     from _3 in Name("左侧升起").Next(Cylinder(MachineDevices.LeftLift).FireAndWait(true))
                     from _4 in Name("左侧旋转180").Next(Motion(MachineDevices.LeftRotate).MoveToAndWait(180))
                     from _5 in Name("左侧下降").Next(Cylinder(MachineDevices.LeftLift).FireAndWait(false))
                     from _6 in Name("左侧夹爪张开").Next(Cylinder(MachineDevices.LeftGrip).FireAndWait(false))
                     from _7 in Name("推拉到右侧").Next(Cylinder(MachineDevices.SlideCyl).FireAndWait(false))
                     from _8 in Name("右侧夹爪闭合").Next(Cylinder(MachineDevices.RightGrip).FireAndWait(true))
                     from _9 in Name("右侧升起").Next(Cylinder(MachineDevices.RightLift).FireAndWait(true))
                     from _10 in Name("右侧旋转180").Next(Motion(MachineDevices.RightRotate).MoveToAndWait(180))
                     from _11 in Name("右侧下降").Next(Cylinder(MachineDevices.RightLift).FireAndWait(false))
                     from _12 in Name("右侧夹爪张开").Next(Cylinder(MachineDevices.RightGrip).FireAndWait(false))
                     select Unit.Default).Definition;

                var model = TransferStationModel.CreateDemo();
                var events = new System.Reactive.Subjects.Subject<SimulationDomainEvent>();

                Action<FlowContext, System.Reactive.Disposables.CompositeDisposable> beforeRun = (ctx, d) =>
                {
                    var leftGrip = ctx.GetDevice<ISimulatorCylinder>(MachineDevices.LeftGrip.Name);
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

                    var rightGrip = ctx.GetDevice<ISimulatorCylinder>(MachineDevices.RightGrip.Name);
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
