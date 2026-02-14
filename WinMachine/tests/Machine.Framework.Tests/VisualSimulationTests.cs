using System;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;
using Machine.Framework.Visualization;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Primitives;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace Machine.Framework.Tests
{
    public class VisualSimulationTests
    {
        [Fact]
        public async Task Prototype_Visual_Flow_Binding_And_Tracking()
        {
            var x = new AxisID("X");
            var z1 = new AxisID("Z1_Axis");

            // 物理层：定义蓝图
            var blueprint = BlueprintScenarios.WinMachineWithDifferentialZ1();
            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);

            // 解释层：使用支持视觉跟踪的解释器 (IVisualFlowInterpreter)
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
                    v.AutoHighlight(pnl_XAxis, x);
                    v.AutoHighlight(pnl_Z1, z1);

                    // 坐标投影绑定
                    v.Bind(pnl_Z1)
                        .ToAxis(z1)
                        .Vertical()
                        .Map(pos => pos * 2); // 比如 1mm 映射为 2像素
                });

            // 执行业务流
            var flow = from _ in Name("初始化动作").Next(Motion(x).MoveToAndWait(100))
                       from __ in Name("笔头下降").Next(Motion(z1).MoveToAndWait(20))
                       select Unit.Default;

            // 运行
            await interpreter.RunAsync(flow.Definition, context);

            Assert.NotNull(flow);
        }

        // --- 辅助测试的 Stub ---
        private class StubVisualInterpreter : IVisualFlowInterpreter
        {
            public IObservable<ActiveStepUpdate> TraceStream => null!; 

            public Task<object?> RunAsync(StepDesc definition, FlowContext context)
            {
                return Task.FromResult<object?>(Unit.Default);
            }
        }

        [Fact]
        public void Verify_Hardware_Shape_DSL_Expressions()
        {
            var z1 = new AxisID("Z1_Slide");
            var r = new AxisID("R_Axis");
            var complex = new AxisID("ComplexArm");
            var push = new CylinderID("PushCylinder");
            var grip = new CylinderID("Gripper");
            var suction = new CylinderID("Suction");

            // 1. 物理蓝图定义
            var blueprint = MachineBlueprint.Define("ShapeDemoMachine")
                .AddBoard("MotionCard", 1, board => 
                {
                    board.AddAxis(z1, 1, a => a.WithRange(0, 100));
                    board.AddAxis(r, 2);
                    board.AddCylinder(push, 0, 0);
                    board.AddCylinder(grip, 1, 1);
                    board.AddCylinder(suction, 0, 0, c => c.WithDynamics(200));
                    board.AddAxis(complex, 3);
                });

            // 2. UI 视觉展现 DSL
            object currentForm = new { Text = "MainSimulator" };
            
            UI.Link(currentForm)
              .Visuals(v => 
              {
                  v.For(z1).AsLinearGuide(200, 20).Vertical().Reversed();
                  v.For(r).AsRotaryTable(radius: 15).Horizontal().Forward();
                  v.For(push).AsSlideBlock().Horizontal().Reversed();
                  v.For(grip).AsGripper(15, 5).Vertical();
                  v.For(suction).AsSuctionPen(diameter: 4).Vertical();
                  v.For(complex).AsCustom("assets/models/robot_arm.obj").Horizontal();
              });

            Assert.NotNull(blueprint);
        }

        [Fact]
        public void Verify_Complex_Assembly_Kinematic_DSL()
        {
            var r = new AxisID("R_Axis");
            var h = new CylinderID("H_Move_Cyl");
            var vLift = new CylinderID("V_Lift_Cyl");
            var gL1 = new CylinderID("Grip_L1");
            var gL2 = new CylinderID("Grip_L2");
            var gR1 = new CylinderID("Grip_R1");
            var gR2 = new CylinderID("Grip_R2");

            // 1. 定义物理蓝图
            var blueprint = MachineBlueprint.Define("RotaryTransferStation")
                .AddBoard("MainCard", 1, board => 
                {
                    board.AddAxis(r, 1);
                    board.AddCylinder(h, 0, 0);
                    board.AddCylinder(vLift, 1, 1);
                    board.AddCylinder(gL1, 2, 2);
                    board.AddCylinder(gL2, 3, 3);
                    board.AddCylinder(gR1, 4, 4);
                    board.AddCylinder(gR2, 5, 5);
                })
                .Mount("RotaryBase", rotary => rotary
                    .LinkTo(r)
                    .Mount("SlideArm", arm => arm
                        .LinkTo(h)
                        .WithOffset(x: 50)
                        .Mount("LiftHead", head => head
                            .LinkTo(vLift)
                            .Mount("Gripper_Group", group => group
                                .Mount("L1").LinkTo(gL1).WithOffset(y: -20)
                                .Mount("L2").LinkTo(gL2).WithOffset(y: -40)
                                .Mount("R1").LinkTo(gR1).WithOffset(y: 20)
                                .Mount("R2").LinkTo(gR2).WithOffset(y: 40)
                            )
                        )
                    )
                );

            // 2. 视觉展现
            UI.Link(new object()) 
              .Visuals(v => 
              {
                  v.For(r).AsRotaryTable(100);
                  v.For(h).AsSlideBlock().Horizontal();
                  v.For(vLift).AsSlideBlock().Vertical();
                  
                  v.For(gL1).AsGripper(10, 2).Vertical();
                  v.For(gL2).AsGripper(10, 2).Vertical();
                  v.For(gR1).AsGripper(10, 2).Vertical();
                  v.For(gR2).AsGripper(10, 2).Vertical();
              });

            Assert.NotNull(blueprint);
        }
    }
}
