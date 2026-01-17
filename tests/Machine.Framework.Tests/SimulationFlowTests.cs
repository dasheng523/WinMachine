using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Core.Configuration.Models;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace Machine.Framework.Tests
{
    public class SimulationFlowTests
    {
        [Fact]
        public async Task Test_Axis_Movement_Simulation_With_Context()
        {
            // 1. 准备配置和上下文
            var config = MachineConfig.Create();
            var context = new FlowContext(config);
            
            var xAxis = new SimulatorAxis("X", 0, 1000, 200);
            context.RegisterDevice("X", xAxis);
            
            var interpreter = new SimulationFlowInterpreter();

            // 2. 定义 DSL
            var moveFlow = 
                from _1 in Motion("X").MoveTo(100)
                from _2 in Motion("X").MoveTo(50)
                select _2;

            // 3. 执行时传入 Context
            await interpreter.RunAsync(moveFlow.Definition, context);

            // 4. 验证
            Assert.Equal(50, xAxis.CurrentState.Position, precision: 1);
            Assert.False(xAxis.CurrentState.IsMoving);
        }

        [Fact]
        public async Task Test_Initialize_From_SimulatorConfig_In_Context()
        {
            // 1. 使用 UseSimulator 替代 UseLeadshine，保持仿真纯净
            var config = MachineConfig.Create()
                .AddControlBoard("SimBoard", b => b
                    .UseSimulator(s => s
                        .MapAxis(MyAxis.X, 0)
                        .ConfigAxis(MyAxis.X, a => a.SetSoftLimits(sl => sl.Range(0, 1000)))
                    )
                );

            // 2. 将 Config 放入 Context
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 3. 定义流程
            var flow = from _ in Motion("X").MoveTo(200)
                       select true;

            // 4. 执行 (解释器应在内部通过 context.Config 进行自动发现)
            await interpreter.RunAsync(flow.Definition, context);
            
            // 验证设备是否由于配置而自动创建
            var xAxis = context.GetDevice<SimulatorAxis>("X");
            Assert.NotNull(xAxis);
            Assert.Equal(200, xAxis.CurrentState.Position, precision: 1);
        }

        private enum MyAxis { X, Y, Z }
    }
}
