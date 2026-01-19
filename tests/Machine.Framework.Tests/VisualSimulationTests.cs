using System;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Simulation;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace Machine.Framework.Tests
{
    public class VisualSimulationTests
    {
        [Fact]
        public async Task Prototype_Visual_Flow_Binding_And_Tracking()
        {
            // 物理层：定义蓝图
            var blueprint = BlueprintScenarios.WinMachineWithDifferentialZ1();
            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);

            // 解释层：使用支持视觉跟踪的解释器 (IVisualFlowInterpreter)
            // 提示：实际开发中，SimulationFlowInterpreter 将实现 IVisualFlowInterpreter
            IVisualFlowInterpreter interpreter = new StubVisualInterpreter();

            // UI层：模拟 WinForms Panel (用 object 代替)
            object pnl_XAxis = new { Name = "pnl_X", Width = 100 };
            object pnl_Z1 = new { Name = "pnl_Z1", Width = 50 };
            object currentForm = new { Text = "MainSimulator" };

                        // --- 绑定 DSL 外观展现（统一入口） ---
                        UI.Link(currentForm)
                            .ObserveInterpreter(interpreter)
                            .Visuals(v =>
                            {
                                    v.AutoHighlight(pnl_XAxis, "X");
                                    v.AutoHighlight(pnl_Z1, "Z1_Axis");

                                    // 坐标投影绑定
                                    v.Bind(pnl_Z1)
                                     .ToAxis("Z1_Axis")
                                     .Vertical()
                                     .Map(pos => pos * 2); // 比如 1mm 映射为 2像素
                            });

            // 执行业务流
            var flow = from _ in Name("初始化动作").Next(Motion("X").MoveToAndWait(100))
                       from __ in Name("笔头下降").Next(Motion("Z1_Axis").MoveToAndWait(20))
                       select Unit.Default;

            // 运行
            await interpreter.RunAsync(flow.Definition, context);

            Assert.NotNull(flow);
            Console.WriteLine("Visual Binding and Flow Tracking DSL Prototype verified.");
        }

        // --- 辅助测试的 Stub ---
        private class StubVisualInterpreter : IVisualFlowInterpreter
        {
            public IObservable<ActiveStepUpdate> TraceStream => null!; // Rx.Observable.Empty 在此省略实现

            public Task<object?> RunAsync(StepDesc definition, FlowContext context)
            {
                // 模拟运行
                return Task.FromResult<object?>(Unit.Default);
            }
        }

        [Fact]
        public void Verify_Hardware_Shape_DSL_Expressions()
        {
            // 1. 物理蓝图定义：只关心逻辑特征、行程和机动性
            var blueprint = MachineSimulator.Assemble("ShapeDemoMachine")
                .AddBoard("MotionCard", 1, board => 
                {
                    board.AddAxis(1, "Z1_Slide").WithRange(0, 100);
                    board.AddAxis(2, "R_Axis");
                    board.AddCylinder("PushCylinder", 0, 0);
                    board.AddCylinder("Gripper", 1, 1);
                    board.AddCylinder("Suction", 2, 2);
                    board.AddAxis(3, "ComplexArm");
                });

            // 2. UI 视觉展现 DSL：定义这些逻辑设备如何从视觉上表达
            // 这种分离允许同一套蓝图有多种视觉表现形式（如 2D Panel 渲染或 3D 模型渲染）
            object currentForm = new { Text = "MainSimulator" };
            
            UI.Link(currentForm)
              .Visuals(v => 
              {
                  // Z轴: 长条+滑块 (竖直, 反向)
                  v.ForAxis("Z1_Slide").AsLinearGuide(200, 20).Vertical().Reversed();

                  // R轴: 旋转座 (水平, 正向)
                  v.ForAxis("R_Axis").AsRotaryTable(radius: 15).Horizontal().Forward();

                  // 气缸: 滑块形态
                  v.ForCylinder("PushCylinder").AsSlideBlock().Horizontal().Reversed();

                  // 气缸: 夹爪形态
                  v.ForCylinder("Gripper").AsGripper(15, 5).Vertical();

                  // 气缸: 吸笔形态
                  v.ForCylinder("Suction").AsSuctionPen(diameter: 4).Vertical();

                  // 复杂部件: 挂载外部模型
                  v.ForAxis("ComplexArm").AsCustom("assets/models/robot_arm.obj").Horizontal();
              });

            Assert.NotNull(blueprint);
        }

        [Fact]
        public void Verify_Complex_Assembly_Kinematic_DSL()
        {
            // 场景：旋转搬运站 (Rotary Transfer Station)
            // 结构：旋转座(R轴) -> 横移气缸 -> 升降气缸 -> 4个夹爪
            
            // 1. 定义物理蓝图 (Kinematic Hierarchy)
            var blueprint = MachineSimulator.Assemble("RotaryTransferStation")
                .AddBoard("MainCard", 1, board => 
                {
                    board.AddAxis(1, "R_Axis");
                    board.AddCylinder("H_Move_Cyl", 0, 0);
                    board.AddCylinder("V_Lift_Cyl", 1, 1);
                    board.AddCylinder("Grip_L1", 2, 2);
                    board.AddCylinder("Grip_L2", 3, 3);
                    board.AddCylinder("Grip_R1", 4, 4);
                    board.AddCylinder("Grip_R2", 5, 5);
                })
                .Mount("RotaryBase", rotary => rotary
                    .LinkTo("R_Axis")
                    .Mount("SlideArm", arm => arm
                        .LinkTo("H_Move_Cyl")
                        .WithOffset(x: 50)
                        .Mount("LiftHead", head => head
                            .LinkTo("V_Lift_Cyl")
                            .Mount("Gripper_Group", group => group
                                .Mount("L1").LinkTo("Grip_L1").WithOffset(y: -20)
                                .Mount("L2").LinkTo("Grip_L2").WithOffset(y: -40)
                                .Mount("R1").LinkTo("Grip_R1").WithOffset(y: 20)
                                .Mount("R2").LinkTo("Grip_R2").WithOffset(y: 40)
                            )
                        )
                    )
                );

            // 2. 视觉展现 (Visual Overlay)
            UI.Link(new object()) // 模拟 Form
              .Visuals(v => 
              {
                  v.ForAxis("R_Axis").AsRotaryTable(100);
                  v.ForCylinder("H_Move_Cyl").AsSlideBlock().Horizontal();
                  v.ForCylinder("V_Lift_Cyl").AsSlideBlock().Vertical();
                  
                  // 批量定义夹爪外观
                  var grippers = new[] { "Grip_L1", "Grip_L2", "Grip_R1", "Grip_R2" };
                  foreach(var g in grippers)
                      v.ForCylinder(g).AsGripper(10, 2).Vertical();
              });

            Assert.NotNull(blueprint);
        }
    }
}
