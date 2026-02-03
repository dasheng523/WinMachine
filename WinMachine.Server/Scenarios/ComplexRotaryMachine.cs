using System;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Interpreters.Configuration;
using Machine.Framework.Visualization;
using System.Collections.Generic;

namespace WinMachine.Server.Scenarios;

/// <summary>
/// 复杂转盘组装机的硬件配置定义类。
/// 负责定义所有的轴、气缸、传感器等硬件资源，以及它们的物理结构关系和可视化模型。
/// </summary>
public sealed class ComplexRotaryMachine
{
    public string Name => "复杂转盘组装场景 (核心逻辑版)";

    // --- 硬件定义 ---
    // 左侧旋转模组硬件
    public readonly CylinderID Cyl_R_Lift = new("Cyl_R_Lift");          // 左侧升降气缸
    public readonly AxisID Axis_R_Table = new("Axis_R_Table");          // 左侧旋转台轴
    public readonly CylinderID Cyl_Grips_Left = new("Cyl_Grips_Left");  // 左侧夹爪气缸

    // 右侧旋转模组硬件
    public readonly CylinderID Cyl_Lift_Right = new("Cyl_Lift_Right");  // 右侧升降气缸
    public readonly AxisID Axis_Table_Right = new("Axis_Table_Right");  // 右侧旋转台轴
    public readonly CylinderID Cyl_Grips_Right = new("Cyl_Grips_Right");// 右侧夹爪气缸

    // 中间搬运模组硬件
    public readonly CylinderID Cyl_Middle_Slide = new("Cyl_Middle_Slide"); // 中间滑台气缸
    
    // 供料（Feeder）模组硬件
    public readonly AxisID Axis_Feeder_X = new("Axis_Feeder_X");        // 供料 X 轴平移
    public readonly AxisID Axis_Feeder_Z1 = new("Axis_Feeder_Z1");      // 供料 Z1 轴升降 (左)
    public readonly AxisID Axis_Feeder_Z2 = new("Axis_Feeder_Z2");      // 供料 Z2 轴升降 (右)
    public readonly CylinderID Vac_Feeder_U1 = new("Vac_Feeder_U1");    // 供料吸笔 U1 (上层)
    public readonly CylinderID Vac_Feeder_L1 = new("Vac_Feeder_L1");    // 供料吸笔 L1 (下层)
    public readonly CylinderID Vac_Feeder_U2 = new("Vac_Feeder_U2");    // 供料吸笔 U2 (上层)
    public readonly CylinderID Vac_Feeder_L2 = new("Vac_Feeder_L2");    // 供料吸笔 L2 (下层)

    /// <summary>
    /// 构建机器的配置、可视化模型和名称。
    /// </summary>
    /// <returns>包含机器配置、可视化模型和名称的元组</returns>
    public (MachineConfig Config, VisualDefinitionModel Model, string Name) Build()
    {
        var bp = MachineBlueprint.Define(Name)
            .AddBoard("MainBoard", 1, b => b.UseSimulator()
                // --- 轴配置 ---
                // 注意：此处所有轴的速度和加速度均被限制为 50，以降低整体运行速度
                .AddAxis(Axis_Feeder_X, 0, a => a.WithRange(-100, 100).WithKinematics(50, 50))
                .AddAxis(Axis_Feeder_Z1, 1, a => a.WithRange(-100, 100).Vertical().WithKinematics(50, 50))
                .AddAxis(Axis_Feeder_Z2, 2, a => a.WithRange(-100, 100).Vertical().WithKinematics(50, 50))
                .AddAxis(Axis_R_Table, 3, a => a.WithRange(-180, 180).WithKinematics(50, 50))
                .AddAxis(Axis_Table_Right, 4, a => a.WithRange(-180, 180).WithKinematics(50, 50))
                
                // --- 气缸配置 ---
                .AddCylinder(Cyl_Middle_Slide, 0, 1)
                .AddCylinder(Cyl_Grips_Left, 2, 3)
                .AddCylinder(Cyl_Grips_Right, 4, 5)
                .AddCylinder(Cyl_R_Lift, 6, 7)
                .AddCylinder(Cyl_Lift_Right, 8, 9)
                .AddCylinder(Vac_Feeder_U1, 10, 11)
                .AddCylinder(Vac_Feeder_L1, 12, 13)
                .AddCylinder(Vac_Feeder_U2, 14, 15)
                .AddCylinder(Vac_Feeder_L2, 16, 17)
            ).Mount("Base", root => root
                // Feeder 模组 (上部供料区)
                // 结构：基座 -> X轴 -> Z轴基座 -> Z轴 -> 吸笔
                .Mount("Feeder_Base", m => m.WithOffset(0, 300, 200)
                    .Mount("Feeder_X", x => x.LinkTo(Axis_Feeder_X).WithStroke(100, 0, 0)
                        .Mount("Feeder_Z1_Base", z1b => z1b.WithOffset(-40, 0, 0)
                            .Mount("Feeder_Z1", z1 => z1.LinkTo(Axis_Feeder_Z1).WithStroke(0, 0, -50)
                                .Mount(Vac_Feeder_U1.Name, u1 => u1.LinkTo(Vac_Feeder_U1).WithOffset(0, 15, 0))
                                .Mount(Vac_Feeder_L1.Name, l1 => l1.LinkTo(Vac_Feeder_L1).WithOffset(0, -15, 0))
                            )
                        )
                        .Mount("Feeder_Z2_Base", z2b => z2b.WithOffset(40, 0, 0)
                            .Mount("Feeder_Z2", z2 => z2.LinkTo(Axis_Feeder_Z2).WithStroke(0, 0, -50)
                                .Mount(Vac_Feeder_U2.Name, u2 => u2.LinkTo(Vac_Feeder_U2).WithOffset(0, 15, 0))
                                .Mount(Vac_Feeder_L2.Name, l2 => l2.LinkTo(Vac_Feeder_L2).WithOffset(0, -15, 0))
                            )
                        )
                    )
                )

                // 中间滑台模组 (负责搬运)
                // 结构：基座 -> 滑板(气缸驱动) -> 吸笔位
                .Mount("Middle_Slide_Base", m => m.WithOffset(0, 0, 50)
                    .Mount("Slide_Plate", s => s.LinkTo(Cyl_Middle_Slide).WithStroke(0, 100, 0)
                        .Mount("Slide_Vac_1", v => v.WithOffset(-40, 0, 20))
                        .Mount("Slide_Vac_2", v => v.WithOffset(40, 0, 20))
                    )
                )

                // 左侧旋转模组
                // 结构：基座 -> 升降(气缸) -> 旋转台(轴) -> 夹爪
                .Mount("Left_Module_Base", m => m.WithOffset(-200, 0, 0)
                    .Mount("L_Lift", l => l.LinkTo(Cyl_R_Lift).WithStroke(0, 0, 50)
                        .Mount("L_Table", t => t.LinkTo(Axis_R_Table)
                            .Mount("L_Grips", g => g.LinkTo(Cyl_Grips_Left).WithOffset(0, 0, 30))
                        )
                    )
                )

                // 右侧旋转模组
                // 结构：基座 -> 升降(气缸) -> 旋转台(轴) -> 夹爪
                .Mount("Right_Module_Base", m => m.WithOffset(200, 0, 0)
                    .Mount("R_Lift", l => l.LinkTo(Cyl_Lift_Right).WithStroke(0, 0, 50)
                        .Mount("R_Table", t => t.LinkTo(Axis_Table_Right)
                            .Mount("R_Grips", g => g.LinkTo(Cyl_Grips_Right).WithOffset(0, 0, 30))
                        )
                    )
                )

                // 静态测试工位 (位于左右两侧)
                // 纯静态位置，无运动机构
                .Mount("Test_Station_Left", t => t.WithOffset(-300, 0, 70)
                    .Mount("Test_Vac_L1", v => v.WithOffset(-30, 0, 0))
                    .Mount("Test_Vac_L2", v => v.WithOffset(30, 0, 0))
                )
                .Mount("Test_Station_Right", t => t.WithOffset(300, 0, 70)
                    .Mount("Test_Vac_R1", v => v.WithOffset(-30, 0, 0))
                    .Mount("Test_Vac_R2", v => v.WithOffset(30, 0, 0))
                )
            );

        var config = BlueprintInterpreter.ToConfig(bp);
        
        // 可视化配置
        // 定义各个部件在前端的显示模型（如丝杆、旋转台、滑块、吸笔等）
        var registry = new CaptureVisualRegistry();
        var layout = Visuals.Start()
            .For(Axis_Feeder_X).AsLinearGuide(100, 20).Done()
            .For(Axis_Feeder_Z1).AsLinearGuide(100, 10).Vertical().Done()
            .For(Axis_Feeder_Z2).AsLinearGuide(100, 10).Vertical().Done()
            .For(Axis_R_Table).AsRotaryTable(50).Done()
            .For(Axis_Table_Right).AsRotaryTable(50).Done()
            .For(Cyl_Middle_Slide).AsSlideBlock(20).Done()
            .For(Vac_Feeder_U1).AsSuctionPen(5).Vertical().Done()
            .For(Vac_Feeder_L1).AsSuctionPen(5).Vertical().Done()
            .For(Vac_Feeder_U2).AsSuctionPen(5).Vertical().Done()
            .For(Vac_Feeder_L2).AsSuctionPen(5).Vertical().Done();

        layout.Build()(registry);

        return (config, registry.Model, Name);
    }
}
