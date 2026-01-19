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
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Tests
{
    public class SimulationFlowTests
    {
        [Fact]
        public async Task Test_Initialize_From_SimulatorConfig_In_Context()
        {
            var axisX = new AxisID("X");
            var config = MachineConfig.Create()
                .AddControlBoard("SimBoard", b => b
                    .MapAxis(axisX.Name, 0)
                    .UseSimulator()
                )
                .ConfigureAxis(axisX.Name, a => a.SetSoftLimits(sl => sl.Range(0, 1000)))
                .UseSimulator("SimBoard", sim => sim.Axis(axisX.Name, a => a.Travel(0, 1000)));

            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 使用 MoveToAndWait 确保阻塞
            var flow = from _1 in Name("X轴回原").Next(Motion(axisX).MoveToAndWait(0))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            var xAxis = context.GetDevice<SimulatorAxis>(axisX.Name);
            Assert.NotNull(xAxis);
            Assert.Equal(0, xAxis.CurrentState.Position);
        }

        [Fact]
        public async Task Test_Dynamic_Calibration_And_Move()
        {
            var axisZ = new AxisID("Z");
            var heightSensor = new SensorID("Height");

            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b
                    .MapAxis(axisZ.Name, 0)
                    .UseSimulator()
                )
                .UseSimulator("Main", sim => sim.Axis(axisZ.Name, a => a.Travel(0, 1000)));
            
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from sensorVal in Name("测高").Next(Sensor(heightSensor).ReadAnalog())
                       let targetPos = 111.0
                       from _move in Name("移动到标定位").Next(Motion(axisZ).MoveToAndWait(targetPos))
                       select targetPos;

            context.Variables["MockValue_Height"] = 5.5;

            var finalPos = await interpreter.RunAsync(flow.Definition, context);

            Assert.Equal(111.0, (double)finalPos!);
            var zAxis = context.GetDevice<SimulatorAxis>(axisZ.Name);
            Assert.NotNull(zAxis);
            Assert.Equal(111.0, zAxis.CurrentState.Position, precision: 1);
        }

        [Fact]
        public async Task Test_Safety_Interlock_Panic_Stop()
        {
            var axisY = new AxisID("Y");
            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b
                    .MapAxis(axisY.Name, 0)
                    .UseSimulator()
                )
                .UseSimulator("Main", sim => sim.Axis(axisY.Name, a => a.Travel(0, 1000)));
            
            using var tcs = new CancellationTokenSource();
            var context = new FlowContext(config, tcs.Token);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from _ in Motion(axisY).MoveToAndWait(1000)
                       select new Unit();

            var task = interpreter.RunAsync(flow.Definition, context);
            await Task.Delay(50);
            tcs.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);

            var yAxis = context.GetDevice<SimulatorAxis>(axisY.Name);
            Assert.NotNull(yAxis);
            Assert.True(yAxis.CurrentState.Position < 1000);
            Assert.False(yAxis.CurrentState.IsMoving);
        }

        [Fact]
        public async Task Test_MoveToObj_Transfer_Sequence()
        {
            var axisRotate = new AxisID("Rotate");
            var gripper = new CylinderID("Gripper");
            var lift = new CylinderID("Lift");
            var vac = new CylinderID("VAC_1");

            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b
                    .MapAxis(axisRotate.Name, 0)
                    .MapCylinder(gripper.Name, 10, extendedPort: 100, retractedPort: 101)
                    .MapCylinder(lift.Name, 11, extendedPort: 102, retractedPort: 103)
                    .MapCylinder(vac.Name, 12)
                    .UseSimulator()
                )
                .ConfigureCylinder(gripper.Name, c => c.MoveTime = 200)
                .ConfigureCylinder(lift.Name, c => c.MoveTime = 200)
                .ConfigureCylinder(vac.Name, c => c.MoveTime = 60)
                .UseSimulator("Main", sim => sim.Axis(axisRotate.Name, a => a.Travel(0, 180)));
            
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            Func<TestSide, Step<Unit>> CreateMoveToObjFlow = (side) =>
            {
                return from _1 in Name("夹取前延时").Next(SystemStep.Delay(100))
                       from _2 in Name("轴动作").Next(Motion(axisRotate).MoveToAndWait(180))
                       from _3 in Name("气缸动作").Next(Cylinder(gripper).FireAndWait(true))
                       select new Unit();
            };

            await interpreter.RunAsync(CreateMoveToObjFlow(TestSide.Left).Definition, context);

            var rotateAxis = context.GetDevice<SimulatorAxis>(axisRotate.Name);
            Assert.NotNull(rotateAxis);
            Assert.Equal(180, rotateAxis.CurrentState.Position, precision: 1);
        }

        [Fact]
        public async Task Test_Pressure_Search_Until_Threshold()
        {
            var axisZ = new AxisID("Z");
            var pressureSensor = new SensorID("Pressure");

            // 验证场景：轴往下移动，压力传感器数值会不断升高，直到达到某个数值后停止移动。
            var config = MachineConfig.Create()
                .AddControlBoard("Main", b => b
                    .MapAxis(axisZ.Name, 0)
                    .UseSimulator()
                )
                .UseSimulator("Main", sim => sim.Axis(axisZ.Name, a => a.Travel(0, 1000)));
            
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 往下推到 200mm，中途压力达到 10.0 时停止
            // 仿真逻辑中：Pressure = (Pos - 100) * 0.5
            // 期望压力在 Pos = 120 时达到 10.0
            
            var flow = from stopPos in Name("寻压探测").Next(Motion(axisZ).MoveUntil(200, pressureSensor, 10.0))
                       select stopPos;

            var resultPos = await interpreter.RunAsync(flow.Definition, context);

            // 验证
            Assert.Equal(120.0, (double)resultPos!, precision: 1);
            
            var zAxis = context.GetDevice<SimulatorAxis>(axisZ.Name);
            Assert.NotNull(zAxis);
            Assert.False(zAxis.CurrentState.IsMoving);
            Assert.Equal(120.0, zAxis.CurrentState.Position, precision: 1);

            // 补充校验：此时压力传感器的数值应当恰好为 10.0
            Assert.True(context.Variables.ContainsKey("MockValue_Pressure"));
            Assert.Equal(10.0, (double)context.Variables["MockValue_Pressure"], precision: 1);
        }

        [Fact]
        public async Task Test_Sensor_Signal_Validation()
        {
            var pressureSensor = new SensorID("Pressure");
            var config = MachineConfig.Create();
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            context.Variables["MockValue_Pressure"] = 5.5;

            var flow = from val in Name("读取压力").Next(Sensor(pressureSensor).ReadAnalog())
                       where val < 10.0
                       select val;

            var result = await interpreter.RunAsync(flow.Definition, context);
            Assert.Equal(5.5, (double)result!);
        }

        [Fact]
        public async Task Test_Blueprint_Driven_Flow_Z1_Linkage()
        {
            var z1Axis = new AxisID("Z1_Axis");
            // 1. 获取物理蓝图 (定义已移至专门的场景类中，避免 LINQ 冲突)
            var blueprint = BlueprintScenarios.WinMachineWithDifferentialZ1();

            // 2. 转换逻辑外部化
            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            // 3. 执行业务流
            var flow = from _ in Name("移动Z1").Next(Motion(z1Axis).MoveToAndWait(10.0))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            // 4. 物理验证
            var z1 = context.GetDevice<SimulatorAxis>(z1Axis.Name);
            Assert.NotNull(z1);
            Assert.Equal(10.0, z1.CurrentState.Position);
        }

        [Fact]
        public async Task Test_Blueprint_Driven_Flow_Cylinder_Safety()
        {
            var clamp = new CylinderID("Clamp");
            var blueprint = BlueprintScenarios.SimpleCylinderWithFeedback();

            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from _ in Name("夹紧").Next(Cylinder(clamp).FireAndWait(true))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            Assert.NotNull(context.GetDevice<CylinderConfig>(clamp.Name));
        }

        public enum TestSide { Left, Right }
    }

    public static class StepTestExtensions
    {
        public static Step<T> Next<TPrevious, T>(this Step<TPrevious> prev, Step<T> next)
        {
            return prev.SelectMany(_ => next, (p, n) => n);
        }
    }
}
