using System;
using System.Linq;
using Xunit;
using Machine.Framework.Core.Flow.Models;
using static Machine.Framework.Core.Flow.Models.FlowBuilders;
using static Machine.Framework.Core.Flow.Models.Step;

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
            Assert.Equal(1000, step.Definition.Policy.Timeout?.TotalMilliseconds);
            Assert.Equal(Handling.AskUser, step.Definition.Policy.ErrorHandling);
        }

        [Fact]
        public void Test_DefineFlow_With_NestedResilience()
        {
            // 验证 DSL 外观与用户需求的一致性
            // 需求：
            // 1. 类似数据库的查询语法 (LINQ)
            // 2. 子步骤失败时可以挂起 (OnError(AskUser))
            // 3. 组合嵌套

            var loadingFlow =
                from start in Name("开始上料")

                // 1. 原子步骤：移动 (自动重试)
                from moved in Motion("Z1").MoveTo(100)
                                          .Retry(3)
                                          .WithTimeout(5000)

                // 2. 复合步骤：吸取组合
                //    使用 Scope 组合多个步骤，并对内部的原子步骤应用特定的错误处理
                from picked in Scope("吸取组合动作", 
                    // 这里传入 sub-flow
                    from cyl in Cylinder("Pen1").Fire(true)
                                                .OnError(Handling.AskUser) // <--- 验证点：原子级人工介入
                    
                    from dly in SystemStep.Delay(500)
                    
                    from vac in Sensor("Vac1").CheckLevel(true)
                                              .Retry(2) // 局部重试 // 验证点：局部策略
                                              .OnError(Handling.AskUser) // 重试失败后人工介入

                    select vac
                )
                
                // 3. 整个 Scope 失败后的兜底策略
                .OnError(Handling.Terminate) 

                // 4. 下一步动作
                from placed in Motion("Z1").MoveTo(0)

                select picked;

            // Assertions
            Assert.NotNull(loadingFlow);
            Assert.NotNull(loadingFlow.Definition);
            
            // 能够一直构建到根节点，证明 SelectMany 链条是通的
            // 最顶层的 Definition 应该是一个 Sequence (因为是从 start 开始的一连串 SelectMany)
            Assert.True(loadingFlow.Definition is SequenceStepDesc, "Result should be a sequence");
            
            var seq = loadingFlow.Definition as SequenceStepDesc;
            Assert.Equal("Sequence", seq.Name);
            // 具体的树结构验证需要递归遍历，这里先验证外观编译通过且非空即可
        }

        [Fact]
        public void Test_Flow_With_Data_Dependency()
        {
            // 验证场景：
            // 1. 读取传感器数值
            // 2. 根据数值进行逻辑分支 (C# if/else 映射)
            
            var pressureFlow = 
                from start in Name("压力测试")

                // 1. 动作：下压
                from _ in Cylinder("Cyl_Press").Fire(true)

                // 2. 获取数据：读取压力
                // 模拟运行时返回 double 值，这里 val 在编译期是 double 类型
                from val in Sensor("Pressure1").ReadAnalog() 

                // 3. 使用数据：根据捕获的 val 动态生成下一步
                from result in val > 50.0 
                    ? Motion("Z1").MoveTo(0)          // 压力达标 -> 抬起
                    : Step.Throw<bool>("压力不足")     // 压力不足 -> 抛出异常

                select val;

            // 验证
            Assert.NotNull(pressureFlow);
            var seq = pressureFlow.Definition as SequenceStepDesc;
            
            // 模拟解释器行为：解构 Sequence
            // 由于 SelectMany 是左结合的，最外层的 Sequence 的 First 实际上包含了前面的所有步骤。
            // 它是 ((Name -> Fire) -> Read) -> Branch
            
            Assert.NotNull(seq);
            Assert.NotNull(seq.First);
            // 只要确认 NextFactory 存在，就证明了 Lambda (包含 if/else 逻辑) 被正确捕获了
            Assert.NotNull(seq.NextFactory);

            // 第二步... 这里解释器通常会运行 First, 拿到结果，然后调用 NextFactory。
            // 我们可以手动调用 NextFactory 模拟这个过程。
            
            // 下面的测试比较 hacky，因为我们手动构建这些 Delegate。
            // 实际上，这个测试主要证明编译通过 + 委托链建立正确。
            
            // 我们无法在这里完整模拟运行时值传递，因为那需要真正的 Monad 绑定。
            // 但我们可以断言 NextFactory 存在，代表 Lambda 被正确封装。
            Assert.NotNull(seq.NextFactory);
        }
    }
}
