using System;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Core.Configuration.Models;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;
using Machine.Framework.Core.Flow.Steps;

namespace Machine.Framework.Tests
{
    public class FlowDslTests
    {
        [Fact]
        public void Test_Step_Policies_Are_Applied_To_Definitions()
        {
            // 简单的原子和策略测试
            var step = Motion("X").MoveTo(10)
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
            // 验证 DSL 外观与用户需求的一致性
            // 需求：
            // 1. 实现一个复杂的组合调度
            // 2. 具备原子级和区域（Scope）级的容错策略
            // 3. 语义连贯，符合工业习惯

            var loadingFlow = 
                from start in Step.Start()
                
                // 1. 先进行一次命名范围内的组合动作
                from picked in Scope("吸取组合动作", 
                    // 这里传入 sub-flow
                    from cyl in Cylinder("Pen1").Fire(true)
                                                .OnError(Handling.AskUser) // 验证点：原子级人工介入
                    
                    from dly in SystemStep.Delay(500)
                    
                    from vac in Sensor("Vac1").CheckLevel(true)
                                              .Retry(2) // 局部重试 // 验证点：局部策略
                                              .OnError(Handling.AskUser) // 重试失败后人工介入

                    select vac
                )
                
                // 3. 整个 Scope 失败后的兜底策略
                .OnError(Handling.Terminate) 

                // 2. 动作结果处理 (Data Dependency)
                from resultMsg in picked 
                    ? Motion("X").MoveTo(50).Select(_ => "SUCCESS")
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
            // 具体的树结构验证需要递归遍历，这里先验证外观编译通过且非空即可
        }

        [Fact]
        public void Test_Flow_With_Data_Dependency()
        {
            // 验证场景：
            // 1. 读取传感器数值
            // 2. 根据数值进行逻辑分支 (C# if/else 映射)
            
            var calibrationFlow = 
                from sensorVal in Sensor("Micrometer").ReadAnalog()
                from status in (sensorVal < 10.5) 
                    ? Motion("Z").MoveTo(sensorVal).Select(_ => "OK")
                    : Motion("Z").MoveTo(0).Select(_ => "FAIL")
                select status;

            Assert.NotNull(calibrationFlow);
            // 这里验证的是 AST 的静态表达能力
        }

        [Fact]
        public async Task Test_SimpleLogInterpreter_Execution()
        {
            // 验证场景：使用解释器运行 DSL
            var flow = from sensorVal in Sensor("In1").ReadAnalog()
                       from action in Motion("X").MoveTo(sensorVal * 2)
                       select "DONE";

            var interpreter = new SimpleLogInterpreter();
            var context = new FlowContext(MachineConfig.Create());

            // 执行
            var result = await interpreter.RunAsync(flow.Definition, context);

            // 验证
            Assert.Equal("DONE", (string)result!);
        }

        [Fact]
        public async Task Test_Control_Flow_Integration()
        {
            // 整合测试：带业务逻辑的解释器运行
            var controlFlow = 
                from val in Sensor("Probe1").ReadAnalog()
                from fr in (val > 50) 
                    ? Step.Start().Select(_ => "OK")
                    : Step.Start().Select(_ => "BAD")
                select fr;

            var interpreter = new SimpleLogInterpreter();
            var context = new FlowContext(MachineConfig.Create());

            // 1. 模拟读数为 55.5 (应该返回 OK)
            var finalResult = await interpreter.RunAsync(controlFlow.Definition, context);

            // 3. 验证结果 (SimpleLogInterpreter 中的 ReadAnalog 模拟返回 55.5)
            Assert.Equal("OK", finalResult);
        }

        [Fact]
        public async Task Test_Complex_Flow_With_Branching_And_Scope()
        {
            // 1. 定义一个可复用的子流程逻辑
            Func<string, Step<bool>> CheckSensor = (id) =>
                from val in Sensor(id).ReadAnalog()
                select val > 50.0;

            // 2. 主流程
            var mainFlow = 
                from _ in Name("主流程开始")
                // 使用子流程
                from isOk in Scope("子流程：传感器检查", CheckSensor("Sensor1"))
                // 根据子流程结果分支
                from status in isOk 
                    ? Motion("X").MoveTo(100).Select(_ => "COMPLETED")
                    : Step.Throw<string>("传感器数值异常")
                select status;

            // 3. 运行
            var context = new FlowContext(MachineConfig.Create());
            var interpreter = new SimpleLogInterpreter();
            var result = await interpreter.RunAsync(mainFlow.Definition, context);
            
            // 4. 断言
            Assert.Equal("COMPLETED", result);
        }
    }
}
