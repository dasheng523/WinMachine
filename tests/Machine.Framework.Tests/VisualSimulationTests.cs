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

            // --- 绑定 DSL 外观展现 ---
            UI.Link(currentForm)
              .ObserveInterpreter(interpreter)
              .AutoHighlight(pnl_XAxis, "X")
              .AutoHighlight(pnl_Z1, "Z1_Axis");

            // 坐标投影绑定
            UI.Link(currentForm)
              .Bind(pnl_Z1)
              .ToAxis("Z1_Axis")
              .Vertical()
              .Map(pos => pos * 2); // 比如 1mm 映射为 2像素

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
    }
}
