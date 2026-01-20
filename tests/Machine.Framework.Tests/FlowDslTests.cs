using System;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Core.Configuration.Models;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;

namespace Machine.Framework.Tests
{
    public class FlowDslTests
    {
        [Fact]
        public void Test_Step_Policies_Are_Applied_To_Definitions()
        {
            var axisX = new AxisID("X");
            // 简单的原子和策略测试
            var step = Motion(axisX).MoveTo(10)
                                  .Retry(3)
                                  .WithTimeout(1000)
                                  .OnError(Handling.AskUser);

            Assert.Equal(3, step.Definition.Policy.RetryCount);
            Assert.Equal(1000, step.Definition.Policy.Timeout.TotalMilliseconds);
            Assert.Equal(Handling.AskUser, step.Definition.Policy.ErrorHandling);
        }

        [Fact]
        public void Test_DefineFlow_With_NestedResilience()
        {
            var pen1 = new CylinderID("Pen1");
            var vac1 = new SensorID("Vac1");
            var axisX = new AxisID("X");

            var loadingFlow = 
                from start in Step.Start()
                
                // 1. 先进行一次命名范围内的组合动作
                from picked in Scope("吸取组合动作", 
                    // 这里传入 sub-flow
                    from cyl in Cylinder(pen1).Fire(true)
                                                 .OnError(Handling.AskUser) // 验证点：原子级人工介入
                    
                    from dly in SystemStep.Delay(500)
                    
                    from vac in Sensor(vac1).CheckLevel(true)
                                              .Retry(2) // 局部重试 // 验证点：局部策略
                                              .OnError(Handling.AskUser) // 重试失败后人工介入
                    
                    select vac
                )
                
                // 3. 整个 Scope 失败后的兜底策略
                .OnError(Handling.Terminate) 

                // 2. 动作结果处理 (Data Dependency)
                from resultMsg in picked 
                    ? Motion(axisX).MoveTo(50).Select(_ => "SUCCESS")
                    : Step.Throw<string>("Pick failed signal check")
                
                select resultMsg;

            // Assertions
            Assert.NotNull(loadingFlow);
            Assert.NotNull(loadingFlow.Definition);
            
            // 能够一直构建到根节点，证明 SelectMany 链条是通的
            // 最顶层的 Definition 应该是一个 Sequence (因为是从 start 开始的一连串 SelectMany)
            Assert.True(loadingFlow.Definition is SequenceStepDesc, "Result should be a sequence");
            
            var seq = loadingFlow.Definition as SequenceStepDesc;
            Assert.NotNull(seq);
            Assert.Equal("Sequence", seq.Name);
        }

        [Fact]
        public void Test_Flow_With_Data_Dependency()
        {
            var micrometer = new SensorID("Micrometer");
            var axisZ = new AxisID("Z");

            // 验证场景：
            // 1. 读取传感器数值
            // 2. 根据数值进行逻辑分支 (C# if/else 映射)
            
            var calibrationFlow = 
                from sensorVal in Sensor(micrometer).ReadAnalog()
                from status in (sensorVal < 10.5) 
                    ? Motion(axisZ).MoveTo(sensorVal).Select(_ => "OK")
                    : Motion(axisZ).MoveTo(0).Select(_ => "FAIL")
                select status;

            Assert.NotNull(calibrationFlow);
            // 这里验证的是 AST 的静态表达能力
        }

        [Fact]
        public async Task Test_SimpleLogInterpreter_Execution()
        {
            var in1 = new SensorID("In1");
            var axisX = new AxisID("X");

            // 验证场景：使用解释器运行 DSL
            var flow = from sensorVal in Sensor(in1).ReadAnalog()
                       from action in Motion(axisX).MoveTo(sensorVal * 2)
                       select "DONE";

            var interpreter = new SimpleLogInterpreter();
            var context = new FlowContext(BlueprintInterpreter.ToConfig(MachineBlueprint.Define("FlowTest").AddBoard("B", 0, b => b.UseSimulator())));

            // 执行
            var result = await interpreter.RunAsync(flow.Definition, context);

            // 验证
            Assert.Equal("DONE", (string)result!);
        }

        [Fact]
        public async Task Test_Control_Flow_Integration()
        {
            var probe1 = new SensorID("Probe1");

            // 整合测试：带业务逻辑的解释器运行
            var controlFlow = 
                from val in Sensor(probe1).ReadAnalog()
                from fr in (val > 50) 
                    ? Step.Start().Select(_ => "OK")
                    : Step.Start().Select(_ => "BAD")
                select fr;

            var interpreter = new SimpleLogInterpreter();
            var context = new FlowContext(BlueprintInterpreter.ToConfig(MachineBlueprint.Define("FlowTest").AddBoard("B", 0, b => b.UseSimulator())));

            // 1. 模拟读数为 55.5 (应该返回 OK)
            var finalResult = await interpreter.RunAsync(controlFlow.Definition, context);

            // 3. 验证结果 (SimpleLogInterpreter 中的 ReadAnalog 模拟返回 55.5)
            Assert.Equal("OK", finalResult);
        }

        [Fact]
        public async Task Test_Complex_Flow_With_Branching_And_Scope()
        {
            var axisX = new AxisID("X");

            // 1. 定义一个可复用的子流程逻辑
            Func<SensorID, Step<bool>> CheckSensor = (id) =>
                from val in Sensor(id).ReadAnalog()
                select val > 50.0;

            // 2. 主流程
            var mainFlow = 
                from _ in Name("主流程开始")
                // 使用子流程
                from isOk in Scope("子流程：传感器检查", CheckSensor(new SensorID("Sensor1")))
                // 根据子流程结果分支
                from status in isOk 
                    ? Motion(axisX).MoveTo(100).Select(_ => "COMPLETED")
                    : Step.Throw<string>("传感器数值异常")
                select status;

            // 3. 运行
            var context = new FlowContext(BlueprintInterpreter.ToConfig(MachineBlueprint.Define("FlowTest").AddBoard("B", 0, b => b.UseSimulator())));
            var interpreter = new SimpleLogInterpreter();
            var result = await interpreter.RunAsync(mainFlow.Definition, context);
            
            // 4. 断言
            Assert.Equal("COMPLETED", result);
        }
    }
}
