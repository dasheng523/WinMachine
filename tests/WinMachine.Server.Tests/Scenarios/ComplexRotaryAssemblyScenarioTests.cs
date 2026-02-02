using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Core.Simulation;
using WinMachine.Server.Scenarios;
using Machine.Framework.Visualization;
using FluentAssertions;

namespace WinMachine.Server.Tests.Scenarios;

public class ComplexRotaryAssemblyScenarioTests
{
    private readonly ITestOutputHelper _output;

    public ComplexRotaryAssemblyScenarioTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private record DeviceSnapshot
    {
        public string StepName { get; init; } = "";
        public StepStatus Status { get; init; }
        public Dictionary<string, double> AxisPositions { get; init; } = new();
        public Dictionary<string, bool> CylinderStates { get; init; } = new();
    }

    [Fact]
    public async Task 校验复杂转盘组装场景_应包含完整的Feeder动作序列()
    {
        var scenario = new ComplexRotaryAssemblyScenario();
        using var cts = new CancellationTokenSource();
        var runtime = scenario.BuildRuntime(cts.Token);
        var interpreter = new SimulationFlowInterpreter();
        
        var snapshots = new ConcurrentBag<DeviceSnapshot>();

        using var sub = interpreter.TraceStream.Subscribe(update => 
        {
            var snapshot = new DeviceSnapshot 
            { 
                StepName = update.Name, 
                Status = update.Status 
            };
            foreach (var device in runtime.Context.Devices)
            {
                if (device.Value is ISimulatorAxis axis) 
                    snapshot.AxisPositions[device.Key] = axis.CurrentState.Position;
                
                if (device.Value is ISimulatorCylinder cyl)
                    snapshot.CylinderStates[device.Key] = cyl.CurrentState.IsExtended;
            }
            snapshots.Add(snapshot);
        });

        var runTask = Task.Run(async () => {
            try {
                await interpreter.RunAsync(runtime.Flow, runtime.Context);
            } catch (OperationCanceledException) { }
        });
        
        var timeout = DateTime.Now.AddSeconds(45); // 增加待机时长
        bool reachedEnd = false;
        while (DateTime.Now < timeout)
        {
            if (snapshots.Any(s => s.StepName == "Feeder: 上料位对齐" && s.Status == StepStatus.Completed))
            {
                reachedEnd = true;
                break;
            }
            await Task.Delay(500);
        }
        
        cts.Cancel();
        try { await runTask; } catch { }

        _output.WriteLine($"Total Snapshots: {snapshots.Count}");
        var snapshotList = snapshots.ToList(); // 转换为列表后再验证，确保存量数据稳定

        try 
        {
            reachedEnd.Should().BeTrue("仿真在规定时间内未到达 'Feeder: 上料位对齐' 完成状态");

            // 1. 验证关键动作是否存在
            var names = snapshotList.Select(s => s.StepName).Distinct().ToList();
            names.Should().Contain(n => n.Contains("Feeder: 下料位对齐"));
            names.Should().Contain(n => n.Contains("Feeder: 上料位对齐"));

            // 2. 验证坐标 (使用 Any 匹配，容忍多周期干扰)
            snapshotList.Any(s => s.StepName == "Feeder: 下料位对齐" && s.Status == StepStatus.Completed && Math.Abs(s.AxisPositions["Axis_Feeder_X"] - (-40)) < 1.0)
                .Should().BeTrue("应存在下料位到达 -40 的完成记录");

            snapshotList.Any(s => s.StepName == "Feeder: 上料位对齐" && s.Status == StepStatus.Completed && Math.Abs(s.AxisPositions["Axis_Feeder_X"] - 40) < 1.0)
                .Should().BeTrue("应存在上料位到达 40 的完成记录");

            // 3. 验证安全互锁: 查找任意一个滑台向前动作的开始点
            var slideFwdStart = snapshotList.FirstOrDefault(s => s.StepName == "滑台向前" && s.Status == StepStatus.Running);
            if (slideFwdStart != null)
            {
                slideFwdStart.AxisPositions["Axis_Feeder_Z1"].Should().BeApproximately(0, 1.0);
                slideFwdStart.AxisPositions["Axis_Feeder_Z2"].Should().BeApproximately(0, 1.0);
            }

            _output.WriteLine(">>> 最终物理逻辑静态验证全部通过。");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"!!! ASSERTION FAILED: {ex.Message}");
            // 打印异常明细辅助诊断
            var badEntries = snapshotList.Where(s => s.StepName == "Feeder: 上料位对齐" && s.Status == StepStatus.Completed).ToList();
            foreach(var entry in badEntries) 
                _output.WriteLine($"Found PlaceAlign at X={entry.AxisPositions["Axis_Feeder_X"]}");
            throw;
        }
    }
}
