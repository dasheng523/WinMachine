using System;
using System.Linq;
using Xunit;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Tests.BlueprintDsl;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Tests
{
    /// <summary>
    /// 全新模拟器蓝图 DSL 的原型验证用例。
    /// 遵循“契约优先”原则，在实现核心逻辑前先确定语法外观。
    /// </summary>
    public class SimulationBlueprintDslTests
    {
        [Fact]
        public void Prototype_Machine_Simulation_Blueprint_Definition()
        {
            var xAxisId = new AxisID("X_Axis");
            var z1AxisId = new AxisID("Z1_Axis");
            var shuttleCylId = new CylinderID("Shuttle_Cyl");

            // 这是我们要讨论的 DSL 外观：
            // 目标：通过 LINQ 表达硬件拓扑、物理挂载以及复杂的轴联动（Differential Linkage）
            
            var blueprint = 
                from machine in MachineSimulator.Assemble("WinMachine_Sim")
                
                // 1. 定义物理板卡资源
                let mainBoard = machine.AddBoard("MainBoard", cardId: 0)
                
                // 2. 定义基础轴 (基准轴)
                let xAxis = mainBoard.AddAxis(id: 0, axis: xAxisId)
                                     .WithKinematics(maxVel: 500, maxAcc: 2000)
                                     .WithRange(min: 0, max: 800)
                
                let z1Axis = mainBoard.AddAxis(id: 1, axis: z1AxisId)
                                      .WithRange(min: -50, max: 50)
                
                // 3. 核心功能：定义物理挂载结构 (Mechanical Hierarchy)
                // 横梁挂在 X 轴上，X 轴移动时，横梁的所有子部件同步位移
                let beam = machine.Mount("MainBeam")
                                  .AttachedTo(xAxis)
                
                // 上料吸笔 1 (LoadingPen1)：
                // - 物理位置：挂在横梁上 (AttachedTo beam)
                // - 动力来源：受 Z1 轴驱动 (LinkTo z1Axis)
                // - 运动方向：同向 (WithTransform z => z)
                let loadingPen1 = machine.Mount("LoadingPen1")
                                         .AttachedTo(beam)
                                         .LinkTo(z1Axis)
                                         .WithTransform(z => z) 
                                         .WithOffset(x: 100, y: 0, z: 0)
                
                // 下料吸笔 1 (UnloadingPen1)：
                // - 物理位置：同样挂在横梁上
                // - 动力来源：受 Z1 轴驱动
                // - 运动方向：反向 (WithTransform z => -z) -> 模拟机械同步带或杠杆
                let unloadingPen1 = machine.Mount("UnloadingPen1")
                                           .AttachedTo(beam)
                                           .LinkTo(z1Axis)
                                           .WithTransform(z => -z) 
                                           .WithOffset(x: 150, y: 0, z: 20) // 初始 z 偏置
                
                // 4. 定义气缸及其关联的 IO 传感器
                let shuttleCyl = mainBoard.AddCylinder(shuttleCylId, doOut: 2, doIn: 3)
                                          .WithFeedback(diOut: 5, diIn: 6)
                                          .WithDynamics(actionTimeMs: 500)
                
                select new { machine, xAxis, z1Axis, loadingPen1, unloadingPen1, shuttleCyl };

            Assert.NotNull(blueprint);
            Assert.Equal("WinMachine_Sim", blueprint.machine.Name);

            Console.WriteLine("Simulation Blueprint DSL Prototype verified.");
        }
    }
}
