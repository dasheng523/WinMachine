using System;
using System.Linq;
using Xunit;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Tests.BlueprintDsl;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Tests
{
    public class SimulationBlueprintDslTests
    {
        [Fact]
        public void Prototype_Machine_Simulation_Blueprint_Definition()
        {
            var xAxisId = new AxisID("X_Axis");
            var z1AxisId = new AxisID("Z1_Axis");
            var shuttleCylId = new CylinderID("Shuttle_Cyl");

            var blueprint = 
                from machine in MachineBlueprint.Define("WinMachine_Sim")
                // 定义板卡并直接在板卡内定义设备，建立物理从属关系
                from mainBoard in machine.AddBoard("MainBoard", cardId: 0, board => {
                    board.AddAxis(xAxisId, 0, a => a.WithKinematics(500, 2000).WithRange(0, 800));
                    board.AddAxis(z1AxisId, 1, a => a.WithRange(-50, 50));
                    board.AddCylinder(shuttleCylId, 2, 3, c => c.WithFeedback(5, 6).WithDynamics(500));
                }).Select(x => x)
                
                // 物理挂载结构 (Mechanical Hierarchy)
                let beam = machine.Mount("MainBeam").AttachedTo(xAxisId)
                let loadingPen1 = machine.Mount("LoadingPen1")
                                         .AttachedTo(beam)
                                         .LinkTo(z1AxisId)
                                         .WithTransform(z => z) 
                                         .WithOffset(x: 100)
                
                select new { machine, loadingPen1 };

            Assert.NotNull(blueprint);
            Assert.Equal("WinMachine_Sim", blueprint.machine.Name);
        }
    }
}
