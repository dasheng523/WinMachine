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

internal sealed class ComplexRotaryAssemblyScenario : IScenarioFactory
{
    public string Name => "Complex_Rotary_Assembly";

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

    private static (Machine.Framework.Core.Configuration.Models.MachineConfig Config, VisualDefinitionModel VisualsModel, string MachineName) BuildConfigAndVisuals()
    {
        var cylR_Lift = new CylinderID("Cyl_R_Lift");
        var axisR_Table = new AxisID("Axis_R_Table");
        var cylGripsLeft = new CylinderID("Cyl_Grips_Left");

        var cylLiftRight = new CylinderID("Cyl_Lift_Right");
        var axisTableRight = new AxisID("Axis_Table_Right");
        var cylGripsRight = new CylinderID("Cyl_Grips_Right");

        var cylMiddleSlide = new CylinderID("Cyl_Middle_Slide");
        var cylMidVac1 = new CylinderID("Cyl_Mid_Vac1");
        var cylMidVac2 = new CylinderID("Cyl_Mid_Vac2");
        var cylMidVac3 = new CylinderID("Cyl_Mid_Vac3");
        var cylMidVac4 = new CylinderID("Cyl_Mid_Vac4");
        
        // 新增上下料机构气缸
        var cylFeederLift = new CylinderID("Cyl_Feeder_Lift");
        var cylFeederGrips = new CylinderID("Cyl_Feeder_Grips");

        var bp = MachineBlueprint.Define("Complex_Rotary_Dual_Assembly")
            .AddBoard("SimCard", 0, b => b.UseSimulator()
                .AddCylinder(cylR_Lift, 0, 0, c => c.WithDynamics(600).Vertical())
                .AddAxis(axisR_Table, 0, a => a.WithRange(0, 360))
                .AddCylinder(cylGripsLeft, 1, 1)

                .AddCylinder(cylLiftRight, 10, 10, c => c.WithDynamics(600).Vertical())
                .AddAxis(axisTableRight, 1, a => a.WithRange(0, 360))
                .AddCylinder(cylGripsRight, 11, 11)

                .AddCylinder(cylMiddleSlide, 20, 20, c => c.WithDynamics(1000).Horizontal())
                .AddCylinder(cylMidVac1, 21, 21)
                .AddCylinder(cylMidVac2, 22, 22)
                .AddCylinder(cylMidVac3, 23, 23)
                .AddCylinder(cylMidVac4, 24, 24)
                
                .AddCylinder(cylFeederLift, 30, 30, c => c.WithDynamics(800).Vertical())
                .AddCylinder(cylFeederGrips, 31, 31))
                .Mount("MachineBase", m => m
                // --- 中央上下料机构 (悬吊式) ---
                .Mount("Central_Feeder_Bridge", bridge => bridge.WithOffset(0, 0, 200) // 悬吊高度调整为 200
                    .Mount("Feeder_Lift", lift => lift.LinkTo(cylFeederLift).WithOffset(0, 0, 0).WithStroke(0, 0, -130) // 下降行程 130 -> 到达 Z=70
                        .Mount("Feeder_Head", head => head.WithOffset(0, 0, 0)
                            // 两个抓手间距 80，对应下方物料座间距
                            .Mount("Feeder_Grip_1", g => g.LinkTo(cylFeederGrips).WithOffset(0, -40, 0))
                            .Mount("Feeder_Grip_2", g => g.LinkTo(cylFeederGrips).WithOffset(0, 40, 0)))))

                // --- 中间滑台 (行程 +/- 65 -> 总行程 130) ---
                // 初始位置(收回): X = +65. Slide Center = +65.
                // 左侧组(Offset -65) -> World X = 0 (对齐中央上下料)
                // 右侧组(Offset +65) -> World X = +130 (对齐右侧模组: 250 - 120 = 130)
                //
                // 动作位置(伸出): Stroke = -130. Slide Center = +65 - 130 = -65.
                // 左侧组(Offset -65) -> World X = -130 (对齐左侧模组: -250 + 120 = -130)
                // 右侧组(Offset +65) -> World X = 0 (对齐中央上下料)
                .Mount("Middle_Module", mid => mid.WithOffset(0, 0, 0)
                    .Mount("Slide_Push", s => s.LinkTo(cylMiddleSlide).WithOffset(65, 0, 0).WithStroke(-130, 0, 0)
                        .Mount("Vac_Plate", p => p.WithOffset(0, 0, 60)
                            .Mount("Vac_Group_L", g => g.WithOffset(-65, 0, 0) 
                                .Mount("Vac1", v => v.LinkTo(cylMidVac1).WithOffset(0, -40, 0))
                                .Mount("Vac2", v => v.LinkTo(cylMidVac2).WithOffset(0, 40, 0)))
                            .Mount("Vac_Group_R", g => g.WithOffset(65, 0, 0) 
                                .Mount("Vac3", v => v.LinkTo(cylMidVac3).WithOffset(0, -40, 0))
                                .Mount("Vac4", v => v.LinkTo(cylMidVac4).WithOffset(0, 40, 0))))))
                .Mount("Assembly_Left", assembly => assembly.WithOffset(x: -250, y: 0, z: 0)
                    .Mount("Lifter_Column", l => l.LinkTo(cylR_Lift).WithOffset(0, 0, 0).WithStroke(0, 0, -50)
                        .Mount("Rotary_Table", r => r.LinkTo(axisR_Table).WithOffset(0, 0, 120)
                            .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)
                                .Mount("Grip_L1", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, -40, 0))
                                .Mount("Grip_L2", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, 40, 0)))
                            .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                .Mount("Grip_R1", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, -40, 0))
                                .Mount("Grip_R2", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, 40, 0))))))
                .Mount("Assembly_Right", assembly => assembly.WithOffset(x: 250, y: 0, z: 0)
                    .Mount("Lifter_Column", l => l.LinkTo(cylLiftRight).WithOffset(0, 0, 0).WithStroke(0, 0, -50)
                        .Mount("Rotary_Table", r => r.LinkTo(axisTableRight).WithOffset(0, 0, 120)
                            .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)
                                .Mount("Grip_L1", grip => grip.LinkTo(cylGripsRight).WithOffset(0, -40, 0))
                                .Mount("Grip_L2", grip => grip.LinkTo(cylGripsRight).WithOffset(0, 40, 0)))
                            .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                .Mount("Grip_R1", grip => grip.LinkTo(cylGripsRight).WithOffset(0, -40, 0))
                                .Mount("Grip_R2", grip => grip.LinkTo(cylGripsRight).WithOffset(0, 40, 0)))))));

        var visuals = Visuals.Define(v =>
        {
            v.For(cylR_Lift).AsSlideBlock(size: 80).Vertical();
            v.For(axisR_Table).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
            v.For(cylGripsLeft).AsGripper(open: 40, close: 10).Horizontal().Reversed();

            v.For(cylLiftRight).AsSlideBlock(size: 80).Vertical();
            v.For(axisTableRight).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
            v.For(cylGripsRight).AsGripper(open: 40, close: 10).Horizontal();

            v.For(cylMiddleSlide).AsSlideBlock(size: 60).Horizontal(); // 恢复为小尺寸，避免遮挡
            v.For(cylMidVac1).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac2).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac3).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac4).AsSuctionPen(diameter: 8).Vertical();

            v.For(cylFeederLift).AsSlideBlock(size: 80).Vertical();
            v.For(cylFeederGrips).AsGripper(open: 40, close: 10).Horizontal();
        });

        var visRegistry = new CaptureVisualRegistry();
        visuals.Build()(visRegistry);

        var config = BlueprintInterpreter.ToConfig(bp);
        var machineName = "Complex Rotary Lift Assembly";

        return (config, visRegistry.Model, machineName);
    }

    private static StepDesc BuildFlow()
    {
        var cylR_Lift = new CylinderID("Cyl_R_Lift");
        var axisR_Table = new AxisID("Axis_R_Table");
        var cylGripsLeft = new CylinderID("Cyl_Grips_Left");

        var cylLiftRight = new CylinderID("Cyl_Lift_Right");
        var axisTableRight = new AxisID("Axis_Table_Right");
        var cylGripsRight = new CylinderID("Cyl_Grips_Right");

        var cylMiddleSlide = new CylinderID("Cyl_Middle_Slide");

            var cycle = 
                from _1 in Name("右侧夹爪闭合").Next(Cylinder(cylGripsRight).FireAndWait(false)) // False=Close
                from _2 in Name("右侧升起").Next(Cylinder(cylLiftRight).FireAndWait(false)) // False=Up (assuming)
                from _3 in Name("右侧旋转到另一头").Next(Motion(axisTableRight).MoveToAndWait(pos => Math.Abs(pos - 0) < 1.0 ? 180 : 0))
                from _4 in Name("右侧降下").Next(Cylinder(cylLiftRight).FireAndWait(true)) // True=Down
                
                from _5 in Name("右侧夹爪松开(放料)").Next(Cylinder(cylGripsRight).FireAndWait(true)) // True=Open

                // --- Phase 2: Transfer Slide ---
                // Pre-Check: Ensure Right Grippers are OPEN before sliding to avoid collision
                // from _check1 in Name("检查:右侧夹爪是否松开").Next(Check(() => Cylinder(cylGripsRight).Is(true), "右侧夹爪必须松开！"))
                from _6 in Name("中间滑台向左").Next(Cylinder(cylMiddleSlide).FireAndWait(true)) 
                // Post-Check: Ensure Slide is at position
                // from _check2 in Name("检查:滑台是否到位").Next(Check(() => Cylinder(cylMiddleSlide).Is(true), "滑台必须到达左侧！"))

                // --- Phase 3: Left Module Pick & Place ---
                from _7 in Name("左侧夹爪闭合").Next(Cylinder(cylGripsLeft).FireAndWait(false))
                from _8 in Name("左侧升起").Next(Cylinder(cylR_Lift).FireAndWait(false))
                from _9 in Name("左侧旋转到另一头").Next(Motion(axisR_Table).MoveToAndWait(pos => Math.Abs(pos - 0) < 1.0 ? 180 : 0))
                from _10 in Name("左侧降下").Next(Cylinder(cylR_Lift).FireAndWait(true))
                from _11 in Name("左侧夹爪松开(放料)").Next(Cylinder(cylGripsLeft).FireAndWait(true))

                from _12 in Name("中间滑台回原位").Next(Cylinder(cylMiddleSlide).FireAndWait(false))

                // --- Reset Phase: Prepare for Next Cycle ---
                // 智能AB位逻辑不需要强制复位
                
                select Unit.Default;

            return cycle.Loop().Definition;
    }
}
