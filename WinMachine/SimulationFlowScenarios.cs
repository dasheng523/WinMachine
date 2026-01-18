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
        Initialize_From_SimulatorConfig_In_Context(),
        Dynamic_Calibration_And_Move(),
        Safety_Interlock_Panic_Stop(),
        MoveToObj_Transfer_Sequence(),
        Pressure_Search_Until_Threshold(),
        Sensor_Signal_Validation(),
        Blueprint_Driven_Flow_Z1_Linkage(),
        Blueprint_Driven_Flow_Cylinder_Safety(),
    ];

    private static SimulationFlowScenario TransferStation_Swap_Left_Then_Right() =>
        new(
            "Demo_TransferStation_Swap_Left_Then_Right",
            cts =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b
                        .MapAxis("Slide", 0)
                        .MapAxis("LeftRotate", 1)
                        .MapAxis("RightRotate", 2)
                        .MapCylinder("LeftLift", 20)
                        .MapCylinder("RightLift", 21)
                        .MapCylinder("LeftGrip", 22)
                        .MapCylinder("RightGrip", 23)
                        .UseSimulator()
                    )
                    .ConfigureAxis("Slide", a => a.SetSoftLimits(sl => sl.Range(-120, 120)))
                    .ConfigureAxis("LeftRotate", a => a.SetSoftLimits(sl => sl.Range(0, 180)))
                    .ConfigureAxis("RightRotate", a => a.SetSoftLimits(sl => sl.Range(0, 180)))
                    .ConfigureCylinder("LeftLift", c => c.WithDynamicsMs(260))
                    .ConfigureCylinder("RightLift", c => c.WithDynamicsMs(260))
                    .ConfigureCylinder("LeftGrip", c => c.WithDynamicsMs(180))
                    .ConfigureCylinder("RightGrip", c => c.WithDynamicsMs(180))
                    .UseSimulator("Main", sim => sim
                        .Axis("Slide", a => a.Travel(-120, 120))
                        .Axis("LeftRotate", a => a.Travel(0, 180))
                        .Axis("RightRotate", a => a.Travel(0, 180))
                        .Timing(t => t.TickMs = 16)
                    );

                var context = new FlowContext(config, cts.Token);

                var flow =
                    (from _1 in Name("推拉到左侧").Next(Motion("Slide").MoveToAndWait(-100))
                     from _2 in Name("左侧夹爪闭合").Next(Cylinder("LeftGrip").FireAndWait(true))
                     from _3 in Name("左侧升起").Next(Cylinder("LeftLift").FireAndWait(true))
                     from _4 in Name("左侧旋转180").Next(Motion("LeftRotate").MoveToAndWait(180))
                     from _5 in Name("左侧下降").Next(Cylinder("LeftLift").FireAndWait(false))
                     from _6 in Name("左侧夹爪张开").Next(Cylinder("LeftGrip").FireAndWait(false))
                     from _7 in Name("推拉到右侧").Next(Motion("Slide").MoveToAndWait(100))
                     from _8 in Name("右侧夹爪闭合").Next(Cylinder("RightGrip").FireAndWait(true))
                     from _9 in Name("右侧升起").Next(Cylinder("RightLift").FireAndWait(true))
                     from _10 in Name("右侧旋转180").Next(Motion("RightRotate").MoveToAndWait(180))
                     from _11 in Name("右侧下降").Next(Cylinder("RightLift").FireAndWait(false))
                     from _12 in Name("右侧夹爪张开").Next(Cylinder("RightGrip").FireAndWait(false))
                     from _13 in Name("推拉回中间").Next(Motion("Slide").MoveToAndWait(0))
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

    private static SimulationFlowScenario Initialize_From_SimulatorConfig_In_Context() =>
        new(
            "Test_Initialize_From_SimulatorConfig_In_Context",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("SimBoard", b => b
                        .MapAxis(MyAxis.X, 0)
                        .UseSimulator()
                    );

                config
                    .ConfigureAxis(MyAxis.X, a => a.SetSoftLimits(sl => sl.Range(0, 1000)))
                    .UseSimulator("SimBoard", sim => sim.Axis(MyAxis.X.ToString(), a => a.Travel(0, 1000)));

                var context = new FlowContext(config);

                var flow =
                    (from _1 in Name("X轴回原").Next(Motion("X").MoveToAndWait(0))
                     select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Dynamic_Calibration_And_Move() =>
        new(
            "Test_Dynamic_Calibration_And_Move",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b
                        .MapAxis(MyAxis.Z, 0)
                        .UseSimulator()
                    )
                    .UseSimulator("Main", sim => sim.Axis(MyAxis.Z.ToString(), a => a.Travel(0, 1000)));

                var context = new FlowContext(config);

                var flow =
                    (from sensorVal in Name("测高").Next(Sensor("Height").ReadAnalog())
                     let targetPos = 111.0
                     from _move in Name("移动到标定位").Next(Motion("Z").MoveToAndWait(targetPos))
                     select targetPos).Definition;

                Action<FlowContext, System.Reactive.Disposables.CompositeDisposable> beforeRun = (_, __) =>
                    context.Variables["MockValue_Height"] = 5.5;

                return new ScenarioRuntime(context, flow, beforeRun, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Safety_Interlock_Panic_Stop() =>
        new(
            "Test_Safety_Interlock_Panic_Stop (auto-cancel 50ms)",
            cts =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b
                        .MapAxis(MyAxis.Y, 0)
                        .UseSimulator()
                    )
                    .UseSimulator("Main", sim => sim.Axis(MyAxis.Y.ToString(), a => a.Travel(0, 1000)));

                var context = new FlowContext(config, cts.Token);

                var flow =
                    (from _ in Motion("Y").MoveToAndWait(1000)
                     select Unit.Default).Definition;

                Action<FlowContext, System.Reactive.Disposables.CompositeDisposable> beforeRun = (_, __) =>
                {
                    System.Threading.Tasks.Task.Run(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(50);
                        cts.Cancel();
                    });
                };

                return new ScenarioRuntime(context, flow, beforeRun, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario MoveToObj_Transfer_Sequence() =>
        new(
            "Test_MoveToObj_Transfer_Sequence",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b
                        .MapAxis(MyAxis.Rotate, 0)
                        .MapCylinder("Gripper", 10, extendedPort: 100, retractedPort: 101)
                        .MapCylinder("Lift", 11, extendedPort: 102, retractedPort: 103)
                        .MapCylinder("VAC_1", 12)
                        .UseSimulator()
                    )
                    .ConfigureCylinder("Gripper", c => c.WithDynamicsMs(200))
                    .ConfigureCylinder("Lift", c => c.WithDynamicsMs(200))
                    .ConfigureCylinder("VAC_1", c => c.WithDynamicsMs(60))
                    .UseSimulator("Main", sim => sim.Axis(MyAxis.Rotate.ToString(), a => a.Travel(0, 180)));

                var context = new FlowContext(config);

                var flow =
                    (from _1 in Name("夹取前延时").Next(SystemStep.Delay(100))
                     from _2 in Name("轴动作").Next(Motion("Rotate").MoveToAndWait(180))
                     from _3 in Name("气缸动作").Next(Cylinder("Gripper").FireAndWait(true))
                     from _4 in Name("真空吸附").Next(Cylinder("VAC_1").FireAndWait(true))
                     select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Pressure_Search_Until_Threshold() =>
        new(
            "Test_Pressure_Search_Until_Threshold",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b
                        .MapAxis(MyAxis.Z, 0)
                        .UseSimulator()
                    )
                    .UseSimulator("Main", sim => sim.Axis(MyAxis.Z.ToString(), a => a.Travel(0, 1000)));

                var context = new FlowContext(config);

                var flow =
                    (from stopPos in Name("寻压探测").Next(Motion("Z").MoveUntil(200, "Pressure", 10.0))
                     select stopPos).Definition;

                return new ScenarioRuntime(context, flow, null, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Sensor_Signal_Validation() =>
        new(
            "Test_Sensor_Signal_Validation",
            _ =>
            {
                var config = MachineConfig.Create();
                var context = new FlowContext(config);

                var flow =
                    (from val in Name("读取压力").Next(Sensor("Pressure").ReadAnalog())
                     where val < 10.0
                     select val).Definition;

                Action<FlowContext, System.Reactive.Disposables.CompositeDisposable> beforeRun = (_, __) =>
                    context.Variables["MockValue_Pressure"] = 5.5;

                return new ScenarioRuntime(context, flow, beforeRun, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Blueprint_Driven_Flow_Z1_Linkage() =>
        new(
            "Test_Blueprint_Driven_Flow_Z1_Linkage",
            _ =>
            {
                var blueprint = WinMachineWithDifferentialZ1();
                var config = BlueprintInterpreter.ToConfig(blueprint);
                var context = new FlowContext(config);

                var flow =
                    (from _1 in Name("移动Z1").Next(Motion("Z1_Axis").MoveToAndWait(10.0))
                     select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Blueprint_Driven_Flow_Cylinder_Safety() =>
        new(
            "Test_Blueprint_Driven_Flow_Cylinder_Safety",
            _ =>
            {
                var blueprint = SimpleCylinderWithFeedback();
                var config = BlueprintInterpreter.ToConfig(blueprint);
                var context = new FlowContext(config);

                var flow =
                    (from _1 in Name("夹紧").Next(Cylinder("Clamp").FireAndWait(true))
                     select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, System.Reactive.Linq.Observable.Empty<SimulationDomainEvent>(), null);
            });

    // ---- 本项目内复制一份蓝图场景（避免 WinMachine 引用 tests 项目） ----

    private static ISimulatorAssemblyBuilder WinMachineWithDifferentialZ1()
    {
        var m = MachineSimulator.Assemble("WinMachine_01");
        var mainBoard = m.AddBoard("MainBoard", cardId: 0);

        var z1Axis = mainBoard.AddAxis(id: 1, name: "Z1_Axis");
        var xAxis = mainBoard.AddAxis(id: 0, name: "X");

        var beam = m.Mount("MainBeam").AttachedTo(xAxis);
        _ = m.Mount("PenLoading").AttachedTo(beam).LinkTo(z1Axis).WithTransform(z => z);
        _ = m.Mount("PenUnloading").AttachedTo(beam).LinkTo(z1Axis).WithTransform(z => -z);

        return m;
    }

    private static ISimulatorAssemblyBuilder SimpleCylinderWithFeedback()
    {
        var m = MachineSimulator.Assemble("WinMachine_02");
        var board = m.AddBoard("IOBoard", cardId: 0);
        _ = board.AddCylinder("Clamp", doOut: 0, doIn: 1)
            .WithFeedback(diOut: 0, diIn: 1)
            .WithDynamics(actionTimeMs: 100);
        return m;
    }

    private enum MyAxis { X, Y, Z, Rotate }
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
