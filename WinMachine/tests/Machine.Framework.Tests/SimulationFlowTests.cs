using System;
using System.Threading.Tasks;
using Xunit;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;
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
            var blueprint = MachineBlueprint.Define("MachineX")
                .AddBoard("SimBoard", 0, b => b
                    .UseSimulator()
                    .AddAxis(axisX, 0, a => a.WithRange(0, 1000))
                );

            var context = new FlowContext(BlueprintInterpreter.ToConfig(blueprint));
            var interpreter = new SimulationFlowInterpreter();

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

            var blueprint = MachineBlueprint.Define("MachineZ")
                .AddBoard("Main", 0, b => b
                    .UseSimulator()
                    .AddAxis(axisZ, 0, a => a.WithRange(0, 1000))
                );
            
            var context = new FlowContext(BlueprintInterpreter.ToConfig(blueprint));
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
            var blueprint = MachineBlueprint.Define("MachineY")
                .AddBoard("Main", 0, b => b
                    .UseSimulator()
                    .AddAxis(axisY, 0, a => a.WithRange(0, 1000))
                );
            
            using var tcs = new CancellationTokenSource();
            var context = new FlowContext(BlueprintInterpreter.ToConfig(blueprint), tcs.Token);
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

            var blueprint = MachineBlueprint.Define("Transfer")
                .AddBoard("Main", 0, b => b
                    .UseSimulator()
                    .AddAxis(axisRotate, 0, a => a.WithRange(0, 180))
                    .AddCylinder(gripper, 10, 11, c => c.WithDynamics(200))
                );
            
            var context = new FlowContext(BlueprintInterpreter.ToConfig(blueprint));
            var interpreter = new SimulationFlowInterpreter();

            Func<Unit, Step<Unit>> CreateMoveToObjFlow = (_) =>
            {
                return from _1 in Name("轴动作").Next(Motion(axisRotate).MoveToAndWait(180))
                       from _2 in Name("气缸动作").Next(Cylinder(gripper).FireAndWait(true))
                       select new Unit();
            };

            await interpreter.RunAsync(CreateMoveToObjFlow(Unit.Default).Definition, context);

            var rotateAxis = context.GetDevice<SimulatorAxis>(axisRotate.Name);
            Assert.NotNull(rotateAxis);
            Assert.Equal(180, rotateAxis.CurrentState.Position, precision: 1);
        }

        [Fact]
        public async Task Test_Pressure_Search_Until_Threshold()
        {
            var axisZ = new AxisID("Z");
            var pressureSensor = new SensorID("Pressure");

            var blueprint = MachineBlueprint.Define("PressureMachine")
                .AddBoard("Main", 0, b => b
                    .UseSimulator()
                    .AddAxis(axisZ, 0, a => a.WithRange(0, 1000))
                );
            
            var context = new FlowContext(BlueprintInterpreter.ToConfig(blueprint));
            var interpreter = new SimulationFlowInterpreter();
            
            var flow = from stopPos in Name("寻压探测").Next(Motion(axisZ).MoveUntil(200, pressureSensor, 10.0))
                       select stopPos;

            var resultPos = await interpreter.RunAsync(flow.Definition, context);

            Assert.Equal(120.0, (double)resultPos!, precision: 1);
        }

        [Fact]
        public async Task Test_Sensor_Signal_Validation()
        {
            var pressureSensor = new SensorID("Pressure");
            var blueprint = MachineBlueprint.Define("Minimal").AddBoard("B", 0, b => b.UseSimulator());
            var context = new FlowContext(BlueprintInterpreter.ToConfig(blueprint));
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
            var z1AxisId = new AxisID("Z1_Axis");
            var blueprint = BlueprintScenarios.WinMachineWithDifferentialZ1();
            var config = BlueprintInterpreter.ToConfig(blueprint);
            var context = new FlowContext(config);
            var interpreter = new SimulationFlowInterpreter();

            var flow = from _ in Name("移动Z1").Next(Motion(z1AxisId).MoveToAndWait(10.0))
                       select new Unit();

            await interpreter.RunAsync(flow.Definition, context);

            var z1 = context.GetDevice<SimulatorAxis>(z1AxisId.Name);
            Assert.NotNull(z1);
            Assert.Equal(10.0, z1.CurrentState.Position);
        }
    }

    public static class StepTestExtensions
    {
        public static Step<T> Next<TPrevious, T>(this Step<TPrevious> prev, Step<T> next)
        {
            return prev.SelectMany(_ => next, (p, n) => n);
        }
    }
}
