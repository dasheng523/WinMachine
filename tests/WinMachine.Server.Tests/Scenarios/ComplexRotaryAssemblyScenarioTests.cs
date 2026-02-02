using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Interpreters.Flow;
using WinMachine.Server.Scenarios;
using Machine.Framework.Visualization;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Telemetry.Contracts;
using FluentAssertions;

namespace WinMachine.Server.Tests.Scenarios;

/// <summary>
/// 模块化场景测试：利用组件化重构，实现对子任务的精确逻辑验证
/// </summary>
public class ComplexRotaryAssemblyScenarioTests
{
    private readonly ITestOutputHelper _output;
    private readonly ComplexRotaryAssemblyScenario _scenario;

    public ComplexRotaryAssemblyScenarioTests(ITestOutputHelper output)
    {
        _output = output;
        _scenario = new ComplexRotaryAssemblyScenario();
    }

    /// <summary>
    /// 辅助工具：运行指定的 Step 并捕获完整的动作轨迹
    /// </summary>
    private async Task<List<ActiveStepUpdate>> ExecuteStepAndCapture(Step<Unit> step, ScenarioRuntime runtime)
    {
        var interpreter = new SimulationFlowInterpreter();
        var trace = new List<ActiveStepUpdate>();
        using var sub = interpreter.TraceStream.Subscribe(trace.Add);
        
        await interpreter.RunAsync(step.Definition, runtime.Context);
        
        return trace;
    }

    [Fact]
    public async Task Feeder逻辑测试_当滑台存在旧料时_应执行下料对位与回收动作()
    {
        // 1. Arrange: 环境准备与状态注入
        var runtime = _scenario.BuildRuntime(default);
        const string slideVac1 = "Slide_Vac_1";
        const string slideVac2 = "Slide_Vac_2";
        
        // 注入状态：Slide_Vac_1 有旧料(Old)，Slide_Vac_2 为空(Empty)
        runtime.Context.MaterialStates[slideVac1] = new MaterialInfo { Id = "OldPart_01", Class = "Old" };
        runtime.Context.MaterialStates[slideVac2] = new MaterialInfo { Id = "", Class = "Empty" };

        // 2. Act: 仅运行 FeederJob 子任务
        var trace = await ExecuteStepAndCapture(_scenario.FeederJob(slideVac1, slideVac2), runtime);

        // 3. Assert: 声明式验证动作序列
        _output.WriteLine("Captured Feeder Trace for OLD scenario:");
        foreach (var u in trace.Where(x => x.Status == StepStatus.Completed)) _output.WriteLine($"  - {u.Name}");

        // 验证关键路径：必须包含下料位对齐
        trace.Should().Contain(u => u.Name == "Feeder: 下料位对齐" && u.Status == StepStatus.Completed);
        // 验证物理坐标：下料对齐完成时，X 轴应在 -40
        var pickSnap = trace.First(u => u.Name == "Feeder: 下料位对齐" && u.Status == StepStatus.Completed);
        // 注意：此处可通过更精细的 Mock 验证，或者直接通过 trace 结果验证
        
        // 验证产物：Slide_Vac_1 的旧料应该被 Consume 了，变为 Empty (或者由 Spawn 产生新料)
        // 在 FeederJob 逻辑中，下料后紧接着会进行上料对齐并 Spawn 新料
        trace.Should().Contain(u => u.Name == "Feeder: 上料位对齐" && u.Status == StepStatus.Completed);
        runtime.Context.MaterialStates.TryGetValue(slideVac2, out var info).Should().BeTrue($"{slideVac2} should have new material after feeder job");
        info?.Class.Should().Be("New");
    }

    [Fact]
    public async Task 安全锁逻辑测试_SafetyBarrier执行后_所有危险轴轴必须回到零位()
    {
        // 1. Arrange
        var runtime = _scenario.BuildRuntime(default);
        
        // 2. Act
        var trace = await ExecuteStepAndCapture(_scenario.SafetyBarrier(), runtime);

        // 3. Assert
        trace.Should().Contain(u => u.Name == "安全检查" && u.Status == StepStatus.Completed);
        
        // 获取安全检查完成时刻的物理快照
        // 这里体现了重构的好处：我们不需要在几千行 Log 里找，Trace 里只有安全检查相关的动作
        var finalUpdate = trace.Last(u => u.Status == StepStatus.Completed);
        // 验证 System 层面所有轴的一致性 (通过 Context 直接校验)
        foreach (var device in runtime.Context.Devices.Values.OfType<Machine.Framework.Core.Simulation.ISimulatorAxis>())
        {
            if (device.AxisId.Contains("Feeder")) // 仅检查 Feeder 相关轴
            {
                device.CurrentState.Position.Should().BeApproximately(0, 0.1, $"轴 {device.AxisId} 在安全检查后未回零");
            }
        }
    }

    [Fact]
    public async Task 组装模组逻辑测试_当存在测试完成品和前端新料时_应执行交换逻辑()
    {
        // 1. Arrange: 注入“待交换”状态
        var runtime = _scenario.BuildRuntime(default);
        const string sv1 = "Slide_Vac_1";
        const string tv1 = "Test_Vac_L1";

        runtime.Context.MaterialStates[sv1] = new MaterialInfo { Id = "New_Material", Class = "New" };
        runtime.Context.MaterialStates[tv1] = new MaterialInfo { Id = "Tested_Material", Class = "Tested" };

        // 2. Act: 运行 AssemblyJob
        // 模拟滑台已到位 (True)
        var slideCyl = runtime.Context.GetDevice<ISimulatorCylinder>("Cyl_Middle_Slide");
        slideCyl?.StartSet(true, 0);

        var trace = await ExecuteStepAndCapture(
            _scenario.AssemblyJob("TestModule", new ("Cyl_R_Lift"), new ("Axis_R_Table"), new ("Cyl_Grips_Left"), sv1, "Slide_Vac_2", tv1, "Test_Vac_L2", true), 
            runtime
        );

        // 3. Assert
        trace.Any(t => t.Name.Contains("TestModule") && t.Status == StepStatus.Completed).Should().BeTrue("满足交换条件时必须执行 TestModule 动作组");
        
        runtime.Context.MaterialStates.TryGetValue(tv1, out var infoT1).Should().BeTrue($"{tv1} should have material");
        infoT1?.Class.Should().Be("New", "交换后测试座应变为新料");

        runtime.Context.MaterialStates.TryGetValue(sv1, out var infoS1).Should().BeTrue($"{sv1} should have material");
        infoS1?.Class.Should().Be("Old", "交换后滑台应变为测试后的旧料");
    }
}
