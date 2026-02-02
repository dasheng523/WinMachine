using System;
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

public enum PartStatus
{
    Empty,
    New,
    Testing,
    Tested,
    Old
}

public sealed class ComplexRotaryAssemblyScenario : IScenarioFactory
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
        
        // --- 核心轴定义 (坐标互换后) ---
        var axisFeederX = new AxisID("Axis_Feeder_X"); // 新增 X 横移轴
        var axisFeederZ1 = new AxisID("Axis_Feeder_Z1");
        var axisFeederZ2 = new AxisID("Axis_Feeder_Z2");
        var vacFeederU1 = new CylinderID("Vac_Feeder_U1");
        var vacFeederL1 = new CylinderID("Vac_Feeder_L1");
        var vacFeederU2 = new CylinderID("Vac_Feeder_U2");
        var vacFeederL2 = new CylinderID("Vac_Feeder_L2");

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
                
                .AddAxis(axisFeederX, 30, a => a.WithRange(-200, 200).WithKinematics(300, 600))
                .AddAxis(axisFeederZ1, 31, a => a.WithRange(-60, 60).WithKinematics(200, 400).Vertical())
                .AddAxis(axisFeederZ2, 32, a => a.WithRange(-60, 60).WithKinematics(200, 400).Vertical())
                .AddCylinder(vacFeederU1, 33, 33)
                .AddCylinder(vacFeederL1, 34, 34)
                .AddCylinder(vacFeederU2, 35, 35)
                .AddCylinder(vacFeederL2, 36, 36))
                .Mount("MachineBase", m => m
                // --- 中央差分横移上下料机构 (X-Axis Bridge) ---
                .Mount("Central_Feeder_Bridge", bridge => bridge.WithOffset(0, 0, 150) 
                    .Mount("Feeder_X_Slide", xSlide => xSlide.LinkTo(axisFeederX)
                        // Z1 组：控制左侧 L1 和右侧 U1
                        .Mount("Z1_Carrier", z1 => z1.WithOffset(-40, 0, 0) // Z1 位于横梁负方向
                            .Mount("Pen_L1", p => p.LinkTo(axisFeederZ1).WithStroke(0, 0, 60).WithRotation(0,0,0) // L 侧：Z- 对应下降
                                .Mount("Vac_L1", v => v.LinkTo(vacFeederL1)))
                            .Mount("Pen_U1", p => p.LinkTo(axisFeederZ1).WithStroke(0, 0, -60).WithOffset(80, 0, 0) // U 侧：偏移 80mm，Z+ 对应下降
                                .Mount("Vac_U1", v => v.LinkTo(vacFeederU1))))
                        // Z2 组：控制左侧 L2 和右侧 U2
                        .Mount("Z2_Carrier", z2 => z2.WithOffset(40, 0, 0) // Z2 位于横梁正方向
                            .Mount("Pen_L2", p => p.LinkTo(axisFeederZ2).WithStroke(0, 0, 60)
                                .Mount("Vac_L2", v => v.LinkTo(vacFeederL2)))
                            .Mount("Pen_U2", p => p.LinkTo(axisFeederZ2).WithStroke(0, 0, -60).WithOffset(80, 0, 0)
                                .Mount("Vac_U2", v => v.LinkTo(vacFeederU2))))))

                // --- 中间滑台 (行程 +/- 65 -> 总行程 130) ---
                // 初始位置(收回): X = +65. Slide Center = +65.
                // 左侧组(Offset -65) -> World X = 0 (对齐中央上下料)
                // 右侧组(Offset +65) -> World X = +130 (对齐右侧模组: 250 - 120 = 130)
                //
                // 左侧组(Offset -65) -> World X = -130 (对齐左侧模组: -250 + 120 = -130)
                // 右侧组(Offset +65) -> World X = 0 (对齐中央上下料)
                // --- 中间滑台 (Y 轴移动，从后往前) ---
                .Mount("Middle_Slide", slide => slide.LinkTo(cylMiddleSlide)
                    .WithOffset(0, 65, 80)      // 初始 Y 偏移
                    .WithStroke(0, -130, 0)     // Y 轴移动
                    .Mount("Slide_Vac_1", v => v.LinkTo(cylMidVac1).WithOffset(-40, 0, 0)) // 内部间距沿 X 排列
                    .Mount("Slide_Vac_2", v => v.LinkTo(cylMidVac2).WithOffset(40, 0, 0))
                    .Mount("Slide_Vac_3", v => v.LinkTo(cylMidVac3).WithOffset(-40, 0, 0))
                    .Mount("Slide_Vac_4", v => v.LinkTo(cylMidVac4).WithOffset(40, 0, 0)))
                .Mount("Assembly_Front", assembly => assembly.WithOffset(x: 0, y: -250, z: 0) // 原 Left -> Front(Y-)
                    .Mount("Lifter_Column", l => l.LinkTo(cylR_Lift).WithStroke(0, 0, 50)
                        .Mount("Rotary_Table", r => r.LinkTo(axisR_Table).WithOffset(0, 0, 60)
                            .Mount("Mount_Back", g => g.WithOffset(x: 0, y: -120, z: 0)   // 对准滑台位
                                .Mount("Grip_Back1", grip => grip.LinkTo(cylGripsLeft).WithOffset(-40, 0, 0))
                                .Mount("Grip_Back2", grip => grip.LinkTo(cylGripsLeft).WithOffset(40, 0, 0)))
                            .Mount("Mount_Front", g => g.WithOffset(x: 0, y: 120, z: 0)   // 对准测试位
                                .Mount("Grip_Front1", grip => grip.LinkTo(cylGripsLeft).WithOffset(-40, 0, 0))
                                .Mount("Grip_Front2", grip => grip.LinkTo(cylGripsLeft).WithOffset(40, 0, 0))))))
                .Mount("Assembly_Back", assembly => assembly.WithOffset(x: 0, y: 250, z: 0) // 原 Right -> Back(Y+)
                    .Mount("Lifter_Column", l => l.LinkTo(cylLiftRight).WithStroke(0, 0, 50)
                        .Mount("Rotary_Table", r => r.LinkTo(axisTableRight).WithOffset(0, 0, 60)
                            .Mount("Mount_Back", g => g.WithOffset(x: 0, y: -120, z: 0)
                                .Mount("Grip_B1", grip => grip.LinkTo(cylGripsRight).WithOffset(-40, 0, 0))
                                .Mount("Grip_B2", grip => grip.LinkTo(cylGripsRight).WithOffset(40, 0, 0)))
                            .Mount("Mount_Front", g => g.WithOffset(x: 0, y: 120, z: 0)
                                .Mount("Grip_F1", grip => grip.LinkTo(cylGripsRight).WithOffset(-40, 0, 0))
                                .Mount("Grip_F2", grip => grip.LinkTo(cylGripsRight).WithOffset(40, 0, 0))))))
                                
                // --- 测试模组 (前后分布) ---
                .Mount("Test_Station_Left", t => t.WithOffset(0, -370, 60)
                    .Mount("Test_Vac_L1", v => v.WithOffset(-40, 0, 0))
                    .Mount("Test_Vac_L2", v => v.WithOffset(40, 0, 0)))
                .Mount("Test_Station_Right", t => t.WithOffset(0, 370, 60)
                    .Mount("Test_Vac_R1", v => v.WithOffset(-40, 0, 0))
                    .Mount("Test_Vac_R2", v => v.WithOffset(40, 0, 0))));

        var visuals = Visuals.Define(v =>
        {
            v.For(cylR_Lift).AsSlideBlock(size: 80).Vertical();
            v.For(axisR_Table).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
            v.For(cylGripsLeft).AsGripper(open: 40, close: 10).Horizontal(); 

            v.For(cylLiftRight).AsSlideBlock(size: 80).Vertical();
            v.For(axisTableRight).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
            v.For(cylGripsRight).AsGripper(open: 40, close: 10).Horizontal();

            v.For(cylMiddleSlide).AsSlideBlock(size: 60).Horizontal(); 
            v.For(cylMidVac1).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac2).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac3).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac4).AsSuctionPen(diameter: 8).Vertical();

            v.For(axisFeederX).AsLinearGuide(length: 200, sliderWidth: 80).Horizontal();
            v.For(axisFeederZ1).AsLinearGuide(length: 60, sliderWidth: 30).Vertical();
            v.For(axisFeederZ2).AsLinearGuide(length: 60, sliderWidth: 30).Vertical();
            v.For(vacFeederU1).AsSuctionPen(diameter: 10).Vertical(); 
            v.For(vacFeederL1).AsSuctionPen(diameter: 10).Vertical(); 
            v.For(vacFeederU2).AsSuctionPen(diameter: 10).Vertical();
            v.For(vacFeederL2).AsSuctionPen(diameter: 10).Vertical();

            // 为测试座添加一些视觉 (复用 SuctionPen 样式)
            // v.For("Test_Vac_L1")... 我们没有 ID 绑定，所以这里无法直接应用 Style，
            // 但前端 SceneGraph 会渲染默认的 MountPoint 球体，这就够了。
            // 或者是添加虚构的 CylinderID 绑定以便显示。
            // 简单起见，我们暂不给测试座加特殊的 Visual，只要能看到 Material 挂在那里就行。
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
        
        var axisFeederX = new AxisID("Axis_Feeder_X");
        var axisFeederZ1 = new AxisID("Axis_Feeder_Z1");
        var axisFeederZ2 = new AxisID("Axis_Feeder_Z2");
        var vacFeederU1 = new CylinderID("Vac_Feeder_U1");
        var vacFeederL1 = new CylinderID("Vac_Feeder_L1");
        var vacFeederU2 = new CylinderID("Vac_Feeder_U2");
        var vacFeederL2 = new CylinderID("Vac_Feeder_L2");
        
        // 滑台真空节点名称
        var sv1 = "Slide_Vac_1"; var sv2 = "Slide_Vac_2";
        var sv3 = "Slide_Vac_3"; var sv4 = "Slide_Vac_4";

        // --- 状态管理: 独立测试计时 ---
        // 使用闭包变量来存储每个工位的"预计测试完成时间"
        var testEndTimes = new System.Collections.Generic.Dictionary<string, System.DateTime>();
        var rand = new System.Random();

        // 辅助方法: 管理测试座的独立状态 (不包含任何机械动作)
        Step<Unit> ManageTest(string vacName)
        {
            return Material(vacName).CheckState().SelectMany(stateStr => 
            {
                if (!Enum.TryParse<PartStatus>(stateStr, out var state)) return Step.NoOp();

                // 状态: 刚收到新料 -> 开始测试
                if (state == PartStatus.New) 
                {
                    var duration = rand.Next(10, 21); // 随机 10-20 秒
                    testEndTimes[vacName] = System.DateTime.Now.AddSeconds(duration);
                    return Scope($"开始测试 {vacName} ({duration}s)", Material(vacName).Transform(PartStatus.Testing.ToString()));
                }
                
                // 状态: 测试中 -> 检查时间
                if (state == PartStatus.Testing)
                {
                    if (testEndTimes.ContainsKey(vacName))
                    {
                        var remaining = (testEndTimes[vacName] - System.DateTime.Now).TotalSeconds;
                        if (remaining <= 0)
                        {
                            return Name($"测试完成 {vacName}").Next(Material(vacName).Transform(PartStatus.Tested.ToString()));
                        }
                    }
                }
                return Step.NoOp();
            }, (a, b) => Unit.Default);
        }

        // --- 子任务: Feeder (X 横移 + 差分双轴四吸笔) ---
        Step<Unit> FeederJob(string vacSlide1, string vacSlide2) 
        {
            return 
                from s1Str in Material(vacSlide1).CheckState()
                from s2Str in Material(vacSlide2).CheckState()
                let s1 = Enum.TryParse<PartStatus>(s1Str, out var v1) ? v1 : PartStatus.Empty
                let s2 = Enum.TryParse<PartStatus>(s2Str, out var v2) ? v2 : PartStatus.Empty
                
                from _act in (s1 != PartStatus.Testing || s2 != PartStatus.Testing) ?
                (
                    // A. 下料对位: 移动 X 使 U1/U2 对齐滑槽
                    from _x1 in Scope("Feeder: 下料位对齐", Motion(axisFeederX).MoveToAndWait(-40)) // U1/U2 在 X+ 区域，整体移动补偿
                    
                    // 下料笔下降 (Z 正向)
                    from _1 in Scope("Feeder: 下料笔下降",
                        Step.InParallel(
                            Motion(axisFeederZ1).MoveToAndWait(50), 
                            Motion(axisFeederZ2).MoveToAndWait(50)))
                    
                    from _u1 in (s1 == PartStatus.Old ? Material(vacSlide1).AttachTo(vacFeederU1.Name, vacSlide1) : Step.NoOp())
                    from _u1c in (s1 == PartStatus.Old ? Material(vacSlide1).Consume() : Step.NoOp())
                    from _u2 in (s2 == PartStatus.Old ? Material(vacSlide2).AttachTo(vacFeederU2.Name, vacSlide2) : Step.NoOp())
                    from _u2c in (s2 == PartStatus.Old ? Material(vacSlide2).Consume() : Step.NoOp())
                    
                    from _2 in Scope("Feeder: 下料笔回位",
                        Step.InParallel(
                            Motion(axisFeederZ1).MoveToAndWait(0),
                            Motion(axisFeederZ2).MoveToAndWait(0)))

                    // B. 上料对位: 移动 X 使 L1/L2 对齐滑槽
                    from _x2 in Scope("Feeder: 上料位对齐", Motion(axisFeederX).MoveToAndWait(40))
                    
                    // 上料笔下降 (Z 负向)
                    from _3 in Scope("Feeder: 上料笔下降",
                        Step.InParallel(
                            Motion(axisFeederZ1).MoveToAndWait(-50),
                            Motion(axisFeederZ2).MoveToAndWait(-50)))
                    
                    from _l1 in (s1 == PartStatus.Empty ? Material(vacSlide1).Spawn($"P_{rand.Next(1000,9999)}", PartStatus.New.ToString()) : Step.NoOp())
                    from _l1d in (s1 == PartStatus.Empty ? Material(vacFeederL1.Name).Detach() : Step.NoOp())
                    from _l2 in (s2 == PartStatus.Empty ? Material(vacSlide2).Spawn($"P_{rand.Next(1000,9999)}", PartStatus.New.ToString()) : Step.NoOp())
                    from _l2d in (s2 == PartStatus.Empty ? Material(vacFeederL2.Name).Detach() : Step.NoOp())

                    from _4 in Scope("Feeder: 上料笔回位",
                        Step.InParallel(
                            Motion(axisFeederZ1).MoveToAndWait(0),
                            Motion(axisFeederZ2).MoveToAndWait(0)))

                    from _refill in Scope("Feeder: 补充物料",
                        Step.InParallel(
                             Material(vacFeederL1.Name).Spawn("Src", PartStatus.New.ToString()),
                             Material(vacFeederL2.Name).Spawn("Src", PartStatus.New.ToString())
                        ))

                    select Unit.Default
                ) : Step.NoOp() 
                select Unit.Default;
        }

        // --- 子任务: Assembly (智能搬运) ---
        Step<Unit> AssemblyJob(string name, CylinderID cylLift, AxisID axisTable, CylinderID cylGrip, 
            string vacSlide1, string vacSlide2, string vacTest1, string vacTest2, bool expectedSlidePos)
        {
            return 
                // 1. 物理互锁: 只有滑块到了指定位置，本模组才允许动作
                from _interlock in Cylinder(cylMiddleSlide).WaitFor(expectedSlidePos)
                
                // 2. 先维护测试座的时间状态 (逻辑步 0ms)
                from _m1 in ManageTest(vacTest1)
                from _m2 in ManageTest(vacTest2)

                // 3. 检查所有相关 Vac 的状态
                from sS1Str in Material(vacSlide1).CheckState()
                from sS2Str in Material(vacSlide2).CheckState()
                from sT1Str in Material(vacTest1).CheckState()
                from sT2Str in Material(vacTest2).CheckState()
                
                let sS1 = Enum.TryParse<PartStatus>(sS1Str, out var vs1) ? vs1 : PartStatus.Empty
                let sS2 = Enum.TryParse<PartStatus>(sS2Str, out var vs2) ? vs2 : PartStatus.Empty
                let sT1 = Enum.TryParse<PartStatus>(sT1Str, out var vt1) ? vt1 : PartStatus.Empty
                let sT2 = Enum.TryParse<PartStatus>(sT2Str, out var vt2) ? vt2 : PartStatus.Empty

                // 4. 判断是否需要执行物理搬运 (Exchange)
                // 条件: (滑块送来了New) 或者 (测试座测完了Tested)
                from _doTransfer in (sS1 == PartStatus.New || sS2 == PartStatus.New || sT1 == PartStatus.Tested || sT2 == PartStatus.Tested) ?
                (
                    // 执行物理交换流程
                    from _log in Scope($"{name}: 执行交换", Step.NoOp())
                    
                    // A. 直接取料 (默认气缸已在水平对齐高度 Down/False)
                    from _g1 in Cylinder(cylGrip).FireAndWait(false) // Close - Pick

                    // 绑定逻辑 (Bind)
                    from _bS1 in (sS1 == PartStatus.New ? Material(vacSlide1).AttachTo(cylGrip.Name, vacSlide1) : Step.NoOp())
                    from _bS1_c in (sS1 == PartStatus.New ? Material(vacSlide1).Consume() : Step.NoOp())
                    from _bS2 in (sS2 == PartStatus.New ? Material(vacSlide2).AttachTo(cylGrip.Name, vacSlide2) : Step.NoOp())
                    from _bS2_c in (sS2 == PartStatus.New ? Material(vacSlide2).Consume() : Step.NoOp())

                    from _bT1 in (sT1 == PartStatus.Tested ? Material(vacTest1).AttachTo(cylGrip.Name, vacTest1) : Step.NoOp())
                    from _bT1_c in (sT1 == PartStatus.Tested ? Material(vacTest1).Consume() : Step.NoOp())
                    from _bT2 in (sT2 == PartStatus.Tested ? Material(vacTest2).AttachTo(cylGrip.Name, vacTest2) : Step.NoOp())
                    from _bT2_c in (sT2 == PartStatus.Tested ? Material(vacTest2).Consume() : Step.NoOp())

                    // B. 升起, 旋转, 下降 (旋转时必须处于安全高度 Up/True)
                    from _u1 in Cylinder(cylLift).FireAndWait(true)  // 升起避障
                    from _rot in Motion(axisTable).MoveToAndWait(pos => System.Math.Abs(pos - 0) < 1.0 ? 180 : 0) // Toggle
                    from _d2 in Cylinder(cylLift).FireAndWait(false) // 下降放料

                    // C. 放料
                    from _g2 in Cylinder(cylGrip).FireAndWait(true) // Open - Place

                    // 生成逻辑 (Unbind/Spawn)
                    // Slide 侧得到 Old/Tested
                    from _pS1 in (sT1 == PartStatus.Tested ? Material(vacSlide1).Spawn("Done_Part", PartStatus.Old.ToString()) : Step.NoOp())
                    from _pS2 in (sT2 == PartStatus.Tested ? Material(vacSlide2).Spawn("Done_Part", PartStatus.Old.ToString()) : Step.NoOp())
                    
                    // Test 侧得到 New
                    from _pT1 in (sS1 == PartStatus.New ? Material(vacTest1).Spawn("Part", PartStatus.New.ToString()) : Step.NoOp())
                    from _pT2 in (sS2 == PartStatus.New ? Material(vacTest2).Spawn("Part", PartStatus.New.ToString()) : Step.NoOp())

                    from _u2 in Cylinder(cylLift).FireAndWait(false) // 动作结束回到水平对齐高度，方便下次直接对接
                    select Unit.Default
                ) : Step.NoOp() 

                select Unit.Default;
        }

        // 安全屏障: 确保所有垂直轴都在 0/False 状态，且 X 轴回到中位
        Step<Unit> SafetyBarrier()
        {
            return Step.InParallel(
                Motion(axisFeederX).MoveToAndWait(0),
                Motion(axisFeederZ1).MoveToAndWait(0),
                Motion(axisFeederZ2).MoveToAndWait(0),
                Cylinder(cylR_Lift).WaitFor(false),
                Cylinder(cylLiftRight).WaitFor(false)
            ).Select(_ => Unit.Default);
        }

        var cycle = 
            from _start in Scope("--- 循环开始 ---", Step.NoOp())
            
            // 0. 初始化
            from _init_mat in Step.InParallel(
                Material(vacFeederL1.Name).Spawn("INIT_P1", PartStatus.New.ToString()),
                Material(vacFeederL2.Name).Spawn("INIT_P2", PartStatus.New.ToString())
            )
            from _init1 in Cylinder(cylGripsLeft).FireAndWait(true)
            from _init2 in Cylinder(cylGripsRight).FireAndWait(true)
            from _init3 in Cylinder(cylR_Lift).FireAndWait(false)
            from _init4 in Cylinder(cylLiftRight).FireAndWait(false)
            
            // --- 阶段 1: 滑台动作 ---
            from _safe1 in Scope("安全检查", SafetyBarrier())
            from _m1 in Scope("滑台向前", Cylinder(cylMiddleSlide).FireAndWait(true))
            
            from _work1 in Step.InParallel(
                Scope("FrontModule", AssemblyJob("前模组", cylR_Lift, axisR_Table, cylGripsLeft, sv1, sv2, "Test_Vac_L1", "Test_Vac_L2", true)),
                Scope("Feeder_Align", FeederJob(sv3, sv4))
            )

            // --- 阶段 2: 滑台动作 ---
            from _safe2 in Scope("安全检查", SafetyBarrier())
            from _m2 in Scope("滑台向后", Cylinder(cylMiddleSlide).FireAndWait(false))
            
            from _work2 in Step.InParallel(
                Scope("Feeder_Align", FeederJob(sv1, sv2)),
                Scope("BackModule", AssemblyJob("后模组", cylLiftRight, axisTableRight, cylGripsRight, sv3, sv4, "Test_Vac_R1", "Test_Vac_R2", false))
            )
            
            select Unit.Default;
            
        return cycle.Loop().Definition;
    }
}
