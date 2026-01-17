using System;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Configuration.Models;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;
using Machine.Framework.Core.Flow.Steps;
using System.Threading;

namespace Machine.Framework.Tests
{
    public class SimulationFlowTests
    {
        [Fact]
        public async Task Test_Initialize_From_SimulatorConfig_In_Context()
        {
            var config = MachineConfig.Create()
                .AddControlBoard("SimBoard", b => b
                    .UseSimulator(s => s
                        .MapAxis(MyAxis.X, 0)
                        .ConfigAxis(MyAxis.X, a => a.SetSoftLimits(sl => sl.Range(0, 1000)))
                    )
                );

            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 使用 MoveToAndWait 确保阻塞
            var flow = from _1 in Name("X轴回原").Next(Motion("X").MoveToAndWait(0))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            var xAxis = context.GetDevice<SimulatorAxis>("X");
            Assert.NotNull(xAxis);
            Assert.Equal(0, xAxis.CurrentState.Position);
        }

        [Fact]
        public async Task Test_Dynamic_Calibration_And_Move()
        {
            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Z, 0)));
            
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from sensorVal in Name("测高").Next(Sensor("Height").ReadAnalog())
                       let targetPos = 111.0
                       from _move in Name("移动到标定位").Next(Motion("Z").MoveToAndWait(targetPos))
                       select targetPos;

            context.Variables["MockValue_Height"] = 5.5;

            var finalPos = await interpreter.RunAsync(flow.Definition, context);

            Assert.Equal(111.0, (double)finalPos!);
            var zAxis = context.GetDevice<SimulatorAxis>("Z");
            Assert.Equal(111.0, zAxis.CurrentState.Position, precision: 1);
        }

        [Fact]
        public async Task Test_Safety_Interlock_Panic_Stop()
        {
            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Y, 0)));
            
            using var tcs = new CancellationTokenSource();
            var context = new FlowContext(config, tcs.Token);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from _ in Motion("Y").MoveToAndWait(1000)
                       select new Unit();

            var task = interpreter.RunAsync(flow.Definition, context);
            await Task.Delay(50);
            tcs.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);

            var yAxis = context.GetDevice<SimulatorAxis>("Y");
            Assert.True(yAxis.CurrentState.Position < 1000);
            Assert.False(yAxis.CurrentState.IsMoving);
        }

        [Fact]
        public async Task Test_MoveToObj_Transfer_Sequence()
        {
            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b.UseSimulator(s => s
                    .MapAxis(MyAxis.Rotate, 0)
                ))
                .AddCylinder("Gripper", c => c.Drive(10).WithSensors(100, 101))
                .AddCylinder("Lift",    c => c.Drive(11).WithSensors(102, 103))
                .AddCylinder("VAC_1",   c => c.Drive(12));
            
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            Func<TestSide, Step<Unit>> CreateMoveToObjFlow = (side) =>
            {
                return from _1 in Name("夹取前延时").Next(SystemStep.Delay(100))
                       from _2 in Name("轴动作").Next(Motion("Rotate").MoveToAndWait(180))
                       from _3 in Name("气缸动作").Next(Cylinder("Gripper").FireAndWait(true))
                       select new Unit();
            };

            await interpreter.RunAsync(CreateMoveToObjFlow(TestSide.Left).Definition, context);

            var rotateAxis = context.GetDevice<SimulatorAxis>("Rotate");
            Assert.Equal(180, rotateAxis.CurrentState.Position, precision: 1);
        }

        [Fact]
        public async Task Test_Pressure_Search_Until_Threshold()
        {
            // 验证场景：轴往下移动，压力传感器数值会不断升高，直到达到某个数值后停止移动。
            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b.UseSimulator(s => s.MapAxis(MyAxis.Z, 0)));
            
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 往下推到 200mm，中途压力达到 10.0 时停止
            // 仿真逻辑中：Pressure = (Pos - 100) * 0.5
            // 期望压力在 Pos = 120 时达到 10.0
            
            var flow = from stopPos in Name("寻压探测").Next(Motion("Z").MoveUntil(200, "Pressure", 10.0))
                       select stopPos;

            var resultPos = await interpreter.RunAsync(flow.Definition, context);

            // 验证
            Assert.Equal(120.0, (double)resultPos!, precision: 1);
            
            var zAxis = context.GetDevice<SimulatorAxis>("Z");
            Assert.False(zAxis.CurrentState.IsMoving);
            Assert.Equal(120.0, zAxis.CurrentState.Position, precision: 1);

            // 补充校验：此时压力传感器的数值应当恰好为 10.0
            Assert.True(context.Variables.ContainsKey("MockValue_Pressure"));
            Assert.Equal(10.0, (double)context.Variables["MockValue_Pressure"], precision: 1);
        }

        [Fact]
        public async Task Test_Sensor_Signal_Validation()
        {
            var config = MachineConfig.Create();
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            context.Variables["MockValue_Pressure"] = 5.5;

            var flow = from val in Name("读取压力").Next(Sensor("Pressure").ReadAnalog())
                       where val < 10.0
                       select val;

            var result = await interpreter.RunAsync(flow.Definition, context);
            Assert.Equal(5.5, (double)result!);
        }

        [Fact]
        public async Task Test_Blueprint_Driven_Flow_Z1_Linkage()
        {
            // 1. 获取物理蓝图 (定义已移至专门的场景类中，避免 LINQ 冲突)
            var blueprint = BlueprintScenarios.WinMachineWithDifferentialZ1();

            // 2. 转换逻辑外部化
            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 3. 执行业务流
            var flow = from _ in Name("移动Z1").Next(Motion("Z1_Axis").MoveToAndWait(10.0))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            // 4. 物理验证
            Assert.Equal(10.0, context.GetDevice<SimulatorAxis>("Z1_Axis").CurrentState.Position);
        }

        [Fact]
        public async Task Test_Blueprint_Driven_Flow_Cylinder_Safety()
        {
            var blueprint = BlueprintScenarios.SimpleCylinderWithFeedback();

            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from _ in Name("夹紧").Next(Cylinder("Clamp").FireAndWait(true))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            Assert.NotNull(context.GetDevice<CylinderConfig>("Clamp"));
        }

        public enum TestSide { Left, Right }
        private enum MyAxis { X, Y, Z, Rotate }
    }

    public static class StepTestExtensions
    {
        public static Step<T> Next<TPrevious, T>(this Step<TPrevious> prev, Step<T> next)
        {
            return prev.SelectMany(_ => next, (p, n) => n);
        }
    }
}
