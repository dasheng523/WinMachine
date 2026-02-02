using System;
using Machine.Framework.Core.Configuration.Models;
using Unit = Machine.Framework.Core.Flow.Dsl.Unit;
using System.Collections.Generic;
using System.Threading;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Interpreters.Configuration;
using Machine.Framework.Telemetry.Schema;
using Machine.Framework.Visualization;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace WinMachine.Server.Scenarios;

public enum PartStatus { Empty, New, Testing, Tested, Old }

public sealed class ComplexRotaryAssemblyScenario : IScenarioFactory
{
    public string Name => "复杂转盘组装场景 (核心逻辑版)";

    // --- 硬件定义 ---
    private readonly CylinderID Cyl_R_Lift = new("Cyl_R_Lift");
    private readonly AxisID Axis_R_Table = new("Axis_R_Table");
    private readonly CylinderID Cyl_Grips_Left = new("Cyl_Grips_Left");

    private readonly CylinderID Cyl_Lift_Right = new("Cyl_Lift_Right");
    private readonly AxisID Axis_Table_Right = new("Axis_Table_Right");
    private readonly CylinderID Cyl_Grips_Right = new("Cyl_Grips_Right");

    private readonly CylinderID Cyl_Middle_Slide = new("Cyl_Middle_Slide");
    
    private readonly AxisID Axis_Feeder_X = new("Axis_Feeder_X");
    private readonly AxisID Axis_Feeder_Z1 = new("Axis_Feeder_Z1");
    private readonly AxisID Axis_Feeder_Z2 = new("Axis_Feeder_Z2");
    private readonly CylinderID Vac_Feeder_U1 = new("Vac_Feeder_U1");
    private readonly CylinderID Vac_Feeder_L1 = new("Vac_Feeder_L1");
    private readonly CylinderID Vac_Feeder_U2 = new("Vac_Feeder_U2");
    private readonly CylinderID Vac_Feeder_L2 = new("Vac_Feeder_L2");

    private readonly Dictionary<string, DateTime> _testEndTimes = new();
    private readonly Random _rand = new();

    public WebMachineModel BuildSchema()
    {
        var (config, visualsModel, machineName) = BuildConfigAndVisuals();
        return WebMachineModelMapper.MapToWebModel(config, visualsModel, machineName);
    }

    public ScenarioRuntime BuildRuntime(CancellationToken ct)
    {
        var (config, visualsModel, machineName) = BuildConfigAndVisuals();
        var flow = BuildFlow();
        var ctx = new FlowContext(config, ct);
        var schema = WebMachineModelMapper.MapToWebModel(config, visualsModel, machineName);
        return new ScenarioRuntime(Name, schema.SchemaVersion ?? "1.0", ctx, flow, schema);
    }

    public Step<Unit> ManageTest(string vacName)
    {
        return Material(vacName).CheckState().SelectMany(stateStr => 
        {
            if (!Enum.TryParse<PartStatus>(stateStr, out var state)) return Step.NoOp();
            if (state == PartStatus.New) 
            {
                var testDuration = 10 + _rand.NextDouble() * 10;
                _testEndTimes[vacName] = DateTime.Now.AddSeconds(testDuration);
                return Material(vacName).Transform(PartStatus.Testing.ToString());
            }
            if (state == PartStatus.Testing && _testEndTimes.TryGetValue(vacName, out var endTime) && DateTime.Now >= endTime)
            {
                return Scope($"测试完成 {vacName}", Material(vacName).Transform(PartStatus.Tested.ToString()));
            }
            return Step.NoOp();
        }, (a, b) => Unit.Default);
    }

    public Step<Unit> FeederJob(string vacSlide1, string vacSlide2) 
    {
        return 
            from s1Str in Material(vacSlide1).CheckState()
            from s2Str in Material(vacSlide2).CheckState()
            let s1 = Enum.TryParse<PartStatus>(s1Str, out var v1) ? v1 : PartStatus.Empty
            let s2 = Enum.TryParse<PartStatus>(s2Str, out var v2) ? v2 : PartStatus.Empty
            from _act in (s1 != PartStatus.Testing || s2 != PartStatus.Testing) ? (
                from _x1 in Scope("Feeder: 下料位对齐", Motion(Axis_Feeder_X).MoveToAndWait(-40))
                from _1 in Scope("Feeder: 下料笔下降", Step.InParallel(Motion(Axis_Feeder_Z1).MoveToAndWait(50), Motion(Axis_Feeder_Z2).MoveToAndWait(50)))
                from _u1 in (s1 == PartStatus.Old ? Material(vacSlide1).AttachTo(Vac_Feeder_U1.Name, vacSlide1).Next(Material(vacSlide1).Consume()) : Step.NoOp())
                from _u2 in (s2 == PartStatus.Old ? Material(vacSlide2).AttachTo(Vac_Feeder_U2.Name, vacSlide2).Next(Material(vacSlide2).Consume()) : Step.NoOp())
                from _2 in Scope("Feeder: 下料笔回位", Step.InParallel(Motion(Axis_Feeder_Z1).MoveToAndWait(0), Motion(Axis_Feeder_Z2).MoveToAndWait(0)))
                from _x2 in Scope("Feeder: 上料位对齐", Motion(Axis_Feeder_X).MoveToAndWait(40))
                from _3 in Scope("Feeder: 上料笔下降", Step.InParallel(Motion(Axis_Feeder_Z1).MoveToAndWait(-50), Motion(Axis_Feeder_Z2).MoveToAndWait(-50)))
                from _l1 in (s1 == PartStatus.Empty ? Material(vacSlide1).Spawn($"P_{_rand.Next(1000,9999)}", PartStatus.New.ToString()).Next(Material(Vac_Feeder_L1.Name).Detach()) : Step.NoOp())
                from _l2 in (s2 == PartStatus.Empty ? Material(vacSlide2).Spawn($"P_{_rand.Next(1000,9999)}", PartStatus.New.ToString()).Next(Material(Vac_Feeder_L2.Name).Detach()) : Step.NoOp())
                from _4 in Scope("Feeder: 上料笔回位", Step.InParallel(Motion(Axis_Feeder_Z1).MoveToAndWait(0), Motion(Axis_Feeder_Z2).MoveToAndWait(0)))
                from _refill in Scope("Feeder: 补充物料", Step.InParallel(Material(Vac_Feeder_L1.Name).Spawn("Src", PartStatus.New.ToString()), Material(Vac_Feeder_L2.Name).Spawn("Src", PartStatus.New.ToString())))
                select Unit.Default
            ) : Step.NoOp() 
            select Unit.Default;
    }

    public Step<Unit> AssemblyJob(string name, CylinderID cylLift, AxisID axisTable, CylinderID cylGrip, 
        string vacSlide1, string vacSlide2, string vacTest1, string vacTest2, bool expectedSlidePos)
    {
        return 
            from _interlock in Cylinder(Cyl_Middle_Slide).WaitFor(expectedSlidePos)
            from _m1 in ManageTest(vacTest1)
            from _m2 in ManageTest(vacTest2)
            from sT1Str in Material(vacTest1).CheckState()
            from sT2Str in Material(vacTest2).CheckState()
            from sS1Str in Material(vacSlide1).CheckState()
            from sS2Str in Material(vacSlide2).CheckState()
            let sT1 = Enum.TryParse<PartStatus>(sT1Str, out var vt1) ? vt1 : PartStatus.Empty
            let sT2 = Enum.TryParse<PartStatus>(sT2Str, out var vt2) ? vt2 : PartStatus.Empty
            let sS1 = Enum.TryParse<PartStatus>(sS1Str, out var vs1) ? vs1 : PartStatus.Empty
            let sS2 = Enum.TryParse<PartStatus>(sS2Str, out var vs2) ? vs2 : PartStatus.Empty
            let needSwap = (sT1 == PartStatus.Tested || sT2 == PartStatus.Tested) && (sS1 == PartStatus.New || sS2 == PartStatus.New)
            from _act in needSwap ? Scope(name, 
                from _u1 in Cylinder(cylLift).FireAndWait(true)
                from _a1 in Motion(axisTable).MoveToAndWait(expectedSlidePos ? 90 : -90)
                from _g1 in Cylinder(cylGrip).FireAndWait(false)
                from _pickO in Step.InParallel(
                    (sT1 == PartStatus.Tested ? Material(vacTest1).AttachTo(cylGrip.Name, vacTest1).Next(Material(vacTest1).Unbind()) : Step.NoOp()),
                    (sT2 == PartStatus.Tested ? Material(vacTest2).AttachTo(cylGrip.Name, vacTest2).Next(Material(vacTest2).Unbind()) : Step.NoOp())
                )
                from _a2 in Motion(axisTable).MoveToAndWait(0)
                from _pickN in Step.InParallel(
                    (sS1 == PartStatus.New ? Material(vacSlide1).AttachTo(cylGrip.Name, vacSlide1).Next(Material(vacSlide1).Unbind()) : Step.NoOp()),
                    (sS2 == PartStatus.New ? Material(vacSlide2).AttachTo(cylGrip.Name, vacSlide2).Next(Material(vacSlide2).Unbind()) : Step.NoOp())
                )
                from _a3 in Motion(axisTable).MoveToAndWait(expectedSlidePos ? 90 : -90)
                from _placeN in Step.InParallel(
                    (sS1 == PartStatus.New ? Material(vacTest1).Bind("Part", PartStatus.New.ToString()).Next(Material(cylGrip.Name).Detach()) : Step.NoOp()),
                    (sS2 == PartStatus.New ? Material(vacTest2).Bind("Part", PartStatus.New.ToString()).Next(Material(cylGrip.Name).Detach()) : Step.NoOp())
                )
                from _a4 in Motion(axisTable).MoveToAndWait(0)
                from _placeO in Step.InParallel(
                    (sS1 == PartStatus.New && sT1 == PartStatus.Tested ? Material(vacSlide1).Bind("Part", PartStatus.Old.ToString()).Next(Material(cylGrip.Name).Detach()) : Step.NoOp()),
                    (sS2 == PartStatus.New && sT2 == PartStatus.Tested ? Material(vacSlide2).Bind("Part", PartStatus.Old.ToString()).Next(Material(cylGrip.Name).Detach()) : Step.NoOp())
                )
                from _g2 in Cylinder(cylGrip).FireAndWait(true)
                from _u2 in Cylinder(cylLift).FireAndWait(false)
                select Unit.Default
            ) : Step.NoOp()
            select Unit.Default;
    }

    public Step<Unit> SafetyBarrier() => Scope("安全检查", Step.InParallel(
        Motion(Axis_Feeder_X).MoveToAndWait(0),
        Motion(Axis_Feeder_Z1).MoveToAndWait(0),
        Motion(Axis_Feeder_Z2).MoveToAndWait(0),
        Cylinder(Cyl_R_Lift).WaitFor(false),
        Cylinder(Cyl_Lift_Right).WaitFor(false)
    ).Next(Step.NoOp()));

    private StepDesc BuildFlow()
    {
        var sv1 = "Slide_Vac_1"; var sv2 = "Slide_Vac_2";
        var sv3 = "Slide_Vac_3"; var sv4 = "Slide_Vac_4";
        var vt1 = "Test_Vac_L1"; var vt2 = "Test_Vac_L2";
        var vt3 = "Test_Vac_R1"; var vt4 = "Test_Vac_R2";
        var cycle = from _start in Scope("--- 循环开始 ---", Step.NoOp())
            from _init in Step.InParallel(Cylinder(Cyl_Grips_Left).FireAndWait(true), Cylinder(Cyl_Grips_Right).FireAndWait(true), Cylinder(Cyl_R_Lift).FireAndWait(false), Cylinder(Cyl_Lift_Right).FireAndWait(false))
            from _s1 in SafetyBarrier()
            from _m1 in Scope("滑台向前", Cylinder(Cyl_Middle_Slide).FireAndWait(true))
            from _w1 in Step.InParallel(AssemblyJob("FrontModule", Cyl_R_Lift, Axis_R_Table, Cyl_Grips_Left, sv1, sv2, vt1, vt2, true), FeederJob(sv3, sv4))
            from _s2 in SafetyBarrier()
            from _m2 in Scope("滑台向后", Cylinder(Cyl_Middle_Slide).FireAndWait(false))
            from _w2 in Step.InParallel(AssemblyJob("BackModule", Cyl_Lift_Right, Axis_Table_Right, Cyl_Grips_Right, sv3, sv4, vt3, vt4, false), FeederJob(sv1, sv2))
            select Unit.Default;
        return cycle.Loop().Definition;
    }

    public (MachineConfig Config, VisualDefinitionModel Model, string Name) BuildConfigAndVisuals()
    {
        var bp = MachineBlueprint.Define(Name)
            .AddBoard("MainBoard", 1, b => b.UseSimulator()
                .AddAxis(Axis_Feeder_X, 0, a => a.WithRange(-100, 100))
                .AddAxis(Axis_Feeder_Z1, 1, a => a.WithRange(-100, 100).Vertical())
                .AddAxis(Axis_Feeder_Z2, 2, a => a.WithRange(-100, 100).Vertical())
                .AddAxis(Axis_R_Table, 3, a => a.WithRange(-180, 180))
                .AddAxis(Axis_Table_Right, 4, a => a.WithRange(-180, 180))
                .AddCylinder(Cyl_Middle_Slide, 0, 1)
                .AddCylinder(Cyl_Grips_Left, 2, 3)
                .AddCylinder(Cyl_Grips_Right, 4, 5)
                .AddCylinder(Cyl_R_Lift, 6, 7)
                .AddCylinder(Cyl_Lift_Right, 8, 9)
                .AddCylinder(Vac_Feeder_U1, 10, 11)
                .AddCylinder(Vac_Feeder_L1, 12, 13)
                .AddCylinder(Vac_Feeder_U2, 14, 15)
                .AddCylinder(Vac_Feeder_L2, 16, 17)
            ).Mount("Base", root => root
                // Feeder 模组 (上部供料区)
                .Mount("Feeder_Base", m => m.WithOffset(0, 300, 200)
                    .Mount("Feeder_X", x => x.LinkTo(Axis_Feeder_X).WithStroke(100, 0, 0)
                        .Mount("Feeder_Z1_Base", z1b => z1b.WithOffset(-40, 0, 0)
                            .Mount("Feeder_Z1", z1 => z1.LinkTo(Axis_Feeder_Z1).WithStroke(0, 0, -50)
                                .Mount(Vac_Feeder_U1.Name, u1 => u1.LinkTo(Vac_Feeder_U1).WithOffset(0, 15, 0))
                                .Mount(Vac_Feeder_L1.Name, l1 => l1.LinkTo(Vac_Feeder_L1).WithOffset(0, -15, 0))
                            )
                        )
                        .Mount("Feeder_Z2_Base", z2b => z2b.WithOffset(40, 0, 0)
                            .Mount("Feeder_Z2", z2 => z2.LinkTo(Axis_Feeder_Z2).WithStroke(0, 0, -50)
                                .Mount(Vac_Feeder_U2.Name, u2 => u2.LinkTo(Vac_Feeder_U2).WithOffset(0, 15, 0))
                                .Mount(Vac_Feeder_L2.Name, l2 => l2.LinkTo(Vac_Feeder_L2).WithOffset(0, -15, 0))
                            )
                        )
                    )
                )

                // 中间滑台模组 (负责搬运)
                .Mount("Middle_Slide_Base", m => m.WithOffset(0, 0, 50)
                    .Mount("Slide_Plate", s => s.LinkTo(Cyl_Middle_Slide).WithStroke(0, 100, 0)
                        .Mount("Slide_Vac_1", v => v.WithOffset(-40, 0, 20))
                        .Mount("Slide_Vac_2", v => v.WithOffset(40, 0, 20))
                    )
                )

                // 左侧旋转模组
                .Mount("Left_Module_Base", m => m.WithOffset(-200, 0, 0)
                    .Mount("L_Lift", l => l.LinkTo(Cyl_R_Lift).WithStroke(0, 0, 50)
                        .Mount("L_Table", t => t.LinkTo(Axis_R_Table)
                            .Mount("L_Grips", g => g.LinkTo(Cyl_Grips_Left).WithOffset(0, 0, 30))
                        )
                    )
                )

                // 右侧旋转模组
                .Mount("Right_Module_Base", m => m.WithOffset(200, 0, 0)
                    .Mount("R_Lift", l => l.LinkTo(Cyl_Lift_Right).WithStroke(0, 0, 50)
                        .Mount("R_Table", t => t.LinkTo(Axis_Table_Right)
                            .Mount("R_Grips", g => g.LinkTo(Cyl_Grips_Right).WithOffset(0, 0, 30))
                        )
                    )
                )

                // 静态测试工位 (位于左右两侧)
                .Mount("Test_Station_Left", t => t.WithOffset(-300, 0, 70)
                    .Mount("Test_Vac_L1", v => v.WithOffset(-30, 0, 0))
                    .Mount("Test_Vac_L2", v => v.WithOffset(30, 0, 0))
                )
                .Mount("Test_Station_Right", t => t.WithOffset(300, 0, 70)
                    .Mount("Test_Vac_R1", v => v.WithOffset(-30, 0, 0))
                    .Mount("Test_Vac_R2", v => v.WithOffset(30, 0, 0))
                )
            );

        var config = BlueprintInterpreter.ToConfig(bp);
        
        // 可视化配置
        var registry = new CaptureVisualRegistry();
        var layout = Visuals.Start()
            .For(Axis_Feeder_X).AsLinearGuide(100, 20).Done()
            .For(Axis_Feeder_Z1).AsLinearGuide(100, 10).Vertical().Done()
            .For(Axis_Feeder_Z2).AsLinearGuide(100, 10).Vertical().Done()
            .For(Axis_R_Table).AsRotaryTable(50).Done()
            .For(Axis_Table_Right).AsRotaryTable(50).Done()
            .For(Cyl_Middle_Slide).AsSlideBlock(20).Done()
            .For(Vac_Feeder_U1).AsSuctionPen(5).Vertical().Done()
            .For(Vac_Feeder_L1).AsSuctionPen(5).Vertical().Done()
            .For(Vac_Feeder_U2).AsSuctionPen(5).Vertical().Done()
            .For(Vac_Feeder_L2).AsSuctionPen(5).Vertical().Done();

        layout.Build()(registry);

        return (config, registry.Model, Name);
    }
}
