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
                    .Mount("Lifter_Column", l => l.LinkTo(cylR_Lift).WithOffset(0, 0, 0).WithStroke(0, 0, 50) // Lift False=0(Down), True=50(Up)
                        .Mount("Rotary_Table", r => r.LinkTo(axisR_Table).WithOffset(0, 0, 60) // Base Z=0, Grip center should be at Z=60 when Lift=0.
                            .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)   // Radius = 120
                                .Mount("Grip_L1", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, -40, 0))
                                .Mount("Grip_L2", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, 40, 0)))
                            .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                .Mount("Grip_R1", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, -40, 0))
                                .Mount("Grip_R2", grip => grip.LinkTo(cylGripsLeft).WithOffset(0, 40, 0))))))
                .Mount("Assembly_Right", assembly => assembly.WithOffset(x: 250, y: 0, z: 0)
                    .Mount("Lifter_Column", l => l.LinkTo(cylLiftRight).WithOffset(0, 0, 0).WithStroke(0, 0, 50)
                        .Mount("Rotary_Table", r => r.LinkTo(axisTableRight).WithOffset(0, 0, 60)
                            .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)
                                .Mount("Grip_L1", grip => grip.LinkTo(cylGripsRight).WithOffset(0, -40, 0))
                                .Mount("Grip_L2", grip => grip.LinkTo(cylGripsRight).WithOffset(0, 40, 0)))
                            .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                .Mount("Grip_R1", grip => grip.LinkTo(cylGripsRight).WithOffset(0, -40, 0))
                                .Mount("Grip_R2", grip => grip.LinkTo(cylGripsRight).WithOffset(0, 40, 0)))))
                                
                // --- 测试座 (Test Stations) ---
                // 位于模组的外侧 (相对于机器中心)。
                // 左模组中心 (-250, 0). 滑台在内侧 (Right side of LeftModule). 测试座应在左侧 (Left side of LeftModule).
                // Offset calculation:
                // Slide Vac contact point: X = -250 + 120 = -130.
                // Test Vac contact point: X = -250 - 120 = -370.
                .Mount("Test_Station_Left", t => t.WithOffset(x: -370, y: 0, z: 60) // 高度与滑台一致 Z=60
                    .Mount("Test_Vac_L1", v => v.WithOffset(0, -40, 0))
                    .Mount("Test_Vac_L2", v => v.WithOffset(0, 40, 0)))

                .Mount("Test_Station_Right", t => t.WithOffset(x: 370, y: 0, z: 60)
                    .Mount("Test_Vac_R1", v => v.WithOffset(0, -40, 0))
                    .Mount("Test_Vac_R2", v => v.WithOffset(0, 40, 0)))));

        var visuals = Visuals.Define(v =>
        {
            v.For(cylR_Lift).AsSlideBlock(size: 80).Vertical();
            v.For(axisR_Table).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
            // 修正夹爪逻辑：用户反馈之前是反的，且习惯 True=Open。
            // 我们加上 Reversed() 恢复之前状态？不，用户说“点击start会马上闭合”，说明初始是 False(Close)。
            // 关键是：我们将在 Start 里显式 Open。
            // 这里的 Reversed 决定的是 Visual 的表现：True 对应 visual Open 还是 Close。
            // 假设 Standard Gripper: True=Open, False=Close.
            v.For(cylGripsLeft).AsGripper(open: 40, close: 10).Horizontal(); 

            v.For(cylLiftRight).AsSlideBlock(size: 80).Vertical();
            v.For(axisTableRight).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
            v.For(cylGripsRight).AsGripper(open: 40, close: 10).Horizontal();

            v.For(cylMiddleSlide).AsSlideBlock(size: 60).Horizontal(); // 恢复为小尺寸，避免遮挡
            v.For(cylMidVac1).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac2).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac3).AsSuctionPen(diameter: 8).Vertical();
            v.For(cylMidVac4).AsSuctionPen(diameter: 8).Vertical();

            v.For(cylFeederLift).AsSlideBlock(size: 40).Vertical(); // 缩小 Feeder 尺寸避免遮挡
            v.For(cylFeederGrips).AsGripper(open: 40, close: 10).Horizontal();

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
        
        // Define Feeder cylinders in local scope
        var cylFeederLift = new CylinderID("Cyl_Feeder_Lift");
        var cylFeederGrips = new CylinderID("Cyl_Feeder_Grips");

        // --- Sub-Workflow: Feeder Logic (Generic for any station) ---
        // Feeder 只有一套升降轴，所以必须同时处理两个工位 (vacInner, vacOuter)
        // 简化逻辑：每次下降都尝试为 Empty 的工位上料 (New)，并把 Old 的工位物料取走 (Consume)。
        // 为了演示效果，我们假设 Feeder 总是先抓取新料（假装它有无限库存），然后下降。
        Step<Unit> FeederJob(string vacInner, string vacOuter) 
        {
            return 
                from _1 in Name($"Feeder:准备[{vacInner},{vacOuter}]").Next(Step.NoOp())
                // 1. Feeder 抓新料 (模拟：直接生成在新料抓手 Feeder_Grip_1/2 上)
                // 这里为了简单，我们假设 Feeder 抓手对应 vacInner 和 vacOuter
                // 我们直接生成物料在 Vac 上（模拟 Feeder 放下的结果），省略 Feeder 自身的抓取细节动画，只做升降动作
                from _2 in Cylinder(cylFeederLift).FireAndWait(true) // 下降
                
                from _3 in Name($"检查工位 {vacInner}").Next(
                     Material(vacInner).CheckState().SelectMany(state => 
                     {
                        // 逻辑：Feeder 下降 -> 发现 Empty -> 生成 -> 抓手抓起(模拟) -> 上升
                        // 修正：Spawn 应该在 Feeder 接触时发生
                        if (state == "Empty") return Material(vacInner).Spawn($"Part_{System.Guid.NewGuid().ToString().Substring(0,4)}", "New");
                        if (state == "Old") return Material(vacInner).Consume();
                        return Step.NoOp();
                     }, (x, y) => Unit.Default)
                )
                from _4 in Name($"检查工位 {vacOuter}").Next(
                     Material(vacOuter).CheckState().SelectMany(state => 
                     {
                        if (state == "Empty") return Material(vacOuter).Spawn($"Part_{System.Guid.NewGuid().ToString().Substring(0,4)}", "New");
                        if (state == "Old") return Material(vacOuter).Consume();
                        return Step.NoOp();
                     }, (x, y) => Unit.Default)
                )
                
                from _5 in Cylinder(cylFeederLift).FireAndWait(false) // 上升
                select Unit.Default;
        }

        // --- Sub-Workflow: Assembly Logic (Generic for any module) ---
        Step<Unit> AssemblyJob(string name, CylinderID cylLift, AxisID axisTable, CylinderID cylGrip, 
            string vacSlide1, string vacSlide2, string vacTest1, string vacTest2)
        {
            // 夹爪映射：
            // 0度时: RightGrips(Grip_R1/2) -> Slide(Inner), LeftGrips(Grip_L1/2) -> Test(Outer)
            // 180度时: LeftGrips -> Slide, RightGrips -> Test
            
            // 逻辑简化：
            // 1. 检查 Slide 是否有新料 (New) 或者 Test 是否有旧料 (Old/Tested) 需要带走
            // 2. 下降抓取 (同时抓 Slide 和 Test)
            // 3. 旋转 (Swap)
            // 4. 下降放料 (Slide 得到 Old, Test 得到 New)
            // 5. 原地模拟测试 5s
            
            return 
                from slide1 in Material(vacSlide1).CheckState()
                from slide2 in Material(vacSlide2).CheckState()
                from test1 in Material(vacTest1).CheckState()
                // 如果 Slide 有新料，或者 Test 有完工料，则执行交换
                from _act in (slide1 == "New" || slide2 == "New" || test1 == "Tested" || test1 == "Old") ? 
                    (
                        from _1 in Name($"{name}:开始交换循环").Next(Step.NoOp())
                        
                        // 1. 获取当前旋转角度以决定 Grip 映射
                        // 由于 DSL 是构建时确定的，我们使用动态 SelectMany 来运行时判断
                        from _dyn in Motion(axisTable).MoveToAndWait(pos => pos) // Dummy move to get pos? No, just use side effect logic or assume toggle.
                        // 为了简化，我们直接执行 "Toggle" 逻辑，并在 Bind 时根据假定位置绑定
                        // 但严谨的做法是：
                        
                        // A. 预备：Lift 必须为 Up (True) - 已由 SafetyBarrier 保证，但这里可以再次确认
                        from _p1 in Cylinder(cylLift).WaitFor(true) // Ensure Up
                        from _p2 in Cylinder(cylGrip).FireAndWait(true) // Ensure Open

                        // B. 下降 (False=Down)
                        from _d1 in Cylinder(cylLift).FireAndWait(false)
                        from _g1 in Cylinder(cylGrip).FireAndWait(false) // Close (Pick)

                        // C. 绑定逻辑 (Bind) -- 视觉 Attach
                        // Slide -> Grip L (假设)
                        // 注意：我们需要知道当前 Grip 到底对应哪个 HoldPoint。
                        // 简单起见，我们 Attach 到 Grip 的 Root Device (cylGrip)，前端会自动挂载到末端
                        // 或者更精确：Mount("Grip_L1") ... 但 DSL 里没有这个 ID。
                        // 妥协：Attach 到 CylinderID，前端寻找名为 "Grip_L1" / "Grip_R1" 的下级节点。
                        // 这里我们分别 Attach parts.
                        
                        from _b1 in (slide1 == "New" ? Material(vacSlide1).AttachTo(cylGrip.Name, "NewPart1") : Step.NoOp())
                        from _b2 in (slide1 == "New" ? Material(vacSlide1).Consume() : Step.NoOp()) // 逻辑上离开 Vac

                        from _b3 in (test1 == "Tested" || test1 == "Old" ? Material(vacTest1).AttachTo(cylGrip.Name, "OldPart1") : Step.NoOp())
                        from _b4 in (test1 == "Tested" || test1 == "Old" ? Material(vacTest1).Consume() : Step.NoOp())

                        // D. 升起 & 旋转
                        from _u1 in Cylinder(cylLift).FireAndWait(true) // Up
                        from _r1 in Motion(axisTable).MoveToAndWait(pos => Math.Abs(pos - 0) < 1.0 ? 180 : 0)

                        // E. 下降 & 放开
                        from _d2 in Cylinder(cylLift).FireAndWait(false) // Down
                        from _g2 in Cylinder(cylGrip).FireAndWait(true) // Open (Place)

                        // F. 解绑逻辑 (Unbind/Place)
                        // Detach actually happens implicitly when we Spawn new items at the target.
                        // Or we can explicitly Detach if we want them to "fall".
                        // But Spawning "New" at target is cleaner for logic state.
                        
                        // Slide 侧得到 OldPart
                        from _p_o1 in (test1 == "Tested" || test1 == "Old" ? Material(vacSlide1).Spawn("Done_Part", "Old") : Step.NoOp())
                        
                        // Test 侧得到 NewPart
                        from _p_n1 in (slide1 == "New" ? Material(vacTest1).Spawn("Transferred_Part", "New") : Step.NoOp())

                        // G. 升起 (这里不需要升起，直接开始测试)
                        // from _u2 in Cylinder(cylLift).FireAndWait(true) 
                        
                        // H. 测试过程 (5秒)
                        from _t1 in (slide1 == "New" ? Name($"{name}:测试中(5s)...").Next(SystemStep.Delay(5000)) : Step.NoOp())
                        
                        // I. 更新 Test Station 状态为 "Tested"
                        from _s1 in (slide1 == "New" ? Material(vacTest1).Transform("Tested") : Step.NoOp())

                        // J. 测试完成，升起离开
                        from _uFinal in Cylinder(cylLift).FireAndWait(true) // Up
                        
                        select Unit.Default
                    ) : Step.NoOp()
                    
                select Unit.Default;
        }

        Step<Unit> SafetyBarrier()
        {
            return Step.InParallel(
                Cylinder(cylFeederLift).WaitFor(false),
                Cylinder(cylR_Lift).WaitFor(false),
                Cylinder(cylLiftRight).WaitFor(false)
            ).Select(_ => Unit.Default);
        }

        var cycle = 
            from _start in Name("--- 循环开始 ---").Next(Step.NoOp())
            
            // 0. Ensure Grippers are Open at Start (First Run Fix)
            from _init1 in Cylinder(cylGripsLeft).FireAndWait(true)
            from _init2 in Cylinder(cylGripsRight).FireAndWait(true)
            
            // 1. 滑台向左 (State A)
            // Safety First: Ensure all Z-axis are UP
            from _safe1 in Name("安全互锁检查").Next(SafetyBarrier())
            
            from _m1 in Name("滑台向左").Next(Cylinder(cylMiddleSlide).FireAndWait(true))
            from _p1 in Step.InParallel(
                Scope("LeftModule_Process", AssemblyJob("左模组", cylR_Lift, axisR_Table, cylGripsLeft, "Vac1", "Vac2", "Test_Vac_L1", "Test_Vac_L2")),
                Scope("Feeder_Right_Group", FeederJob("Vac3", "Vac4"))
            )

            // 2. 滑台向右 (State B)
            // Safety First
            from _safe2 in Name("安全互锁检查").Next(SafetyBarrier())

            from _m2 in Name("滑台向右").Next(Cylinder(cylMiddleSlide).FireAndWait(false))
            from _p2 in Step.InParallel(
                Scope("Feeder_Left_Group", FeederJob("Vac1", "Vac2")),
                Scope("RightModule_Process", AssemblyJob("右模组", cylLiftRight, axisTableRight, cylGripsRight, "Vac3", "Vac4", "Test_Vac_R1", "Test_Vac_R2"))
            )

            select Unit.Default;

        return cycle.Loop().Definition;
    }
}
