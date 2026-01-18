using System;
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
    Func<CancellationTokenSource, (FlowContext Context, StepDesc Flow, Action? BeforeRun)> Create);

internal static class SimulationFlowScenarios
{
    public static SimulationFlowScenario[] All =>
    [
        Initialize_From_SimulatorConfig_In_Context(),
        Dynamic_Calibration_And_Move(),
        Safety_Interlock_Panic_Stop(),
        MoveToObj_Transfer_Sequence(),
        Pressure_Search_Until_Threshold(),
        Sensor_Signal_Validation(),
        Blueprint_Driven_Flow_Z1_Linkage(),
        Blueprint_Driven_Flow_Cylinder_Safety(),
    ];

    private static SimulationFlowScenario Initialize_From_SimulatorConfig_In_Context() =>
        new(
            "Test_Initialize_From_SimulatorConfig_In_Context",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("SimBoard", b => b
                        .UseSimulator(s => s
                            .MapAxis(MyAxis.X, 0)
                            .ConfigAxis(MyAxis.X, a => a.SetSoftLimits(sl => sl.Range(0, 1000)))
                        )
                    );

                var context = new FlowContext(config);

                var flow =
                    (from _1 in Name("X轴回原").Next(Motion("X").MoveToAndWait(0))
                     select Unit.Default).Definition;

                return (context, flow, null);
            });

    private static SimulationFlowScenario Dynamic_Calibration_And_Move() =>
        new(
            "Test_Dynamic_Calibration_And_Move",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Z, 0)));

                var context = new FlowContext(config);

                var flow =
                    (from sensorVal in Name("测高").Next(Sensor("Height").ReadAnalog())
                     let targetPos = 111.0
                     from _move in Name("移动到标定位").Next(Motion("Z").MoveToAndWait(targetPos))
                     select targetPos).Definition;

                Action beforeRun = () => context.Variables["MockValue_Height"] = 5.5;
                return (context, flow, beforeRun);
            });

    private static SimulationFlowScenario Safety_Interlock_Panic_Stop() =>
        new(
            "Test_Safety_Interlock_Panic_Stop (auto-cancel 50ms)",
            cts =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Y, 0)));

                var context = new FlowContext(config, cts.Token);

                var flow =
                    (from _ in Motion("Y").MoveToAndWait(1000)
                     select Unit.Default).Definition;

                Action beforeRun = () =>
                {
                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(50);
                        cts.Cancel();
                    });
                };

                return (context, flow, beforeRun);
            });

    private static SimulationFlowScenario MoveToObj_Transfer_Sequence() =>
        new(
            "Test_MoveToObj_Transfer_Sequence",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Rotate, 0)))
                    .AddCylinder("Gripper", c => c.Drive(10).WithSensors(100, 101).WithDynamicsMs(200))
                    .AddCylinder("Lift", c => c.Drive(11).WithSensors(102, 103).WithDynamicsMs(200))
                    .AddCylinder("VAC_1", c => c.Drive(12).WithDynamicsMs(60));

                var context = new FlowContext(config);

                var flow =
                    (from _1 in Name("夹取前延时").Next(SystemStep.Delay(100))
                     from _2 in Name("轴动作").Next(Motion("Rotate").MoveToAndWait(180))
                     from _3 in Name("气缸动作").Next(Cylinder("Gripper").FireAndWait(true))
                     from _4 in Name("真空吸附").Next(Cylinder("VAC_1").FireAndWait(true))
                     select Unit.Default).Definition;

                return (context, flow, null);
            });

    private static SimulationFlowScenario Pressure_Search_Until_Threshold() =>
        new(
            "Test_Pressure_Search_Until_Threshold",
            _ =>
            {
                var config = MachineConfig.Create()
                    .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Z, 0)));

                var context = new FlowContext(config);

                var flow =
                    (from stopPos in Name("寻压探测").Next(Motion("Z").MoveUntil(200, "Pressure", 10.0))
                     select stopPos).Definition;

                return (context, flow, null);
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

                Action beforeRun = () => context.Variables["MockValue_Pressure"] = 5.5;
                return (context, flow, beforeRun);
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

                return (context, flow, null);
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

                return (context, flow, null);
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
