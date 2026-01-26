using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Interpreters.Configuration;

namespace Machine.Framework.Tests.WebParams
{
    using Machine.Framework.Visualization;

    public class WebSchemaExportTests
    {
        private readonly ITestOutputHelper _output;

        public WebSchemaExportTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // --- Mock Devices ---
        public static readonly AxisID Test_Rotary = new AxisID("Test_Rotary");
        public static readonly CylinderID C1_Left_Grip1 = new CylinderID("C1_Left_Grip1");
        public static readonly CylinderID C1_Left_Grip2 = new CylinderID("C1_Left_Grip2");
        public static readonly CylinderID C2_Right_Grip1 = new CylinderID("C2_Right_Grip1");
        public static readonly CylinderID C2_Right_Grip2 = new CylinderID("C2_Right_Grip2");

        // --- WinMachine Full IDs ---
        public static readonly AxisID Axis_X = new AxisID("Axis_X");
        public static readonly AxisID Axis_Y1 = new AxisID("Axis_Y1"); // Unload Tray
        public static readonly AxisID Axis_Y2 = new AxisID("Axis_Y2"); // Load Tray
        public static readonly AxisID Axis_Z1 = new AxisID("Axis_Z1");
        public static readonly AxisID Axis_Z2 = new AxisID("Axis_Z2");
        public static readonly AxisID Axis_R = new AxisID("Axis_R");   // Rotation for Gripper

        public static readonly CylinderID Cyl_LoadPen1 = new CylinderID("Cyl_LoadPen1");
        public static readonly CylinderID Cyl_UnloadPen1 = new CylinderID("Cyl_UnloadPen1");
        public static readonly CylinderID Cyl_LoadPen2 = new CylinderID("Cyl_LoadPen2");
        public static readonly CylinderID Cyl_UnloadPen2 = new CylinderID("Cyl_UnloadPen2");
        public static readonly CylinderID Cyl_ScanPush = new CylinderID("Cyl_ScanPush");
        public static readonly CylinderID Cyl_GripperClamp = new CylinderID("Cyl_GripperClamp");

        public static readonly CylinderID Cyl_GripperLift = new CylinderID("Cyl_GripperLift"); // Pneumatic Lift
        public static readonly CylinderID Cyl_ScanVac1 = new CylinderID("Cyl_ScanVac1");
        public static readonly CylinderID Cyl_ScanVac2 = new CylinderID("Cyl_ScanVac2");
        public static readonly CylinderID Cyl_ScanVac3 = new CylinderID("Cyl_ScanVac3");
        public static readonly CylinderID Cyl_ScanVac4 = new CylinderID("Cyl_ScanVac4");

        public static readonly CylinderID Cyl_Mid_Vac3 = new CylinderID("Cyl_Mid_Vac3");
        public static readonly CylinderID Cyl_Mid_Vac4 = new CylinderID("Cyl_Mid_Vac4");

        [Fact]
        public void Generate_Web_Json_For_Rotary_Grippers()
        {
            // 1. 构建蓝图 (复刻自 Rotary_Dual_Pair_Grippers)
            var bp = MachineBlueprint.Define("Test_Rotary_Grippers")
                .AddBoard("Main", 0, b => b.UseSimulator()
                    .AddAxis(Test_Rotary, 0, a => a.WithRange(0, 360))
                    .AddCylinder(C1_Left_Grip1, 10, 10)
                    .AddCylinder(C1_Left_Grip2, 11, 11)
                    .AddCylinder(C2_Right_Grip1, 12, 12)
                    .AddCylinder(C2_Right_Grip2, 13, 13))
                .Mount("Machine", m => m
                    .Mount("Disk", d => d
                        .LinkTo(Test_Rotary)
                        // 左右各安装一对
                        .Mount("Left_Pair_1", g => g.LinkTo(C1_Left_Grip1).WithOffset(x: -80, y: -30))
                        .Mount("Left_Pair_2", g => g.LinkTo(C1_Left_Grip2).WithOffset(x: -80, y: 30))
                        .Mount("Right_Pair_1", g => g.LinkTo(C2_Right_Grip1).WithOffset(x: 80, y: -30))
                        .Mount("Right_Pair_2", g => g.LinkTo(C2_Right_Grip2).WithOffset(x: 80, y: 30))
                    )
                );

            // 1.1 定义可视化 (UI DSL)
            var visuals = Visuals.Define(v =>
            {
                v.For(Test_Rotary).AsRotaryTable(radius: 120).WithPivot(0.5, 0.5);
                v.For(C1_Left_Grip1).AsGripper(open: 30, close: 10).Vertical();
                v.For(C1_Left_Grip2).AsGripper(open: 30, close: 10).Vertical();
                v.For(C2_Right_Grip1).AsSuctionPen(diameter: 8).Vertical(); // 故意用吸笔以测试不同类型
                v.For(C2_Right_Grip2).AsSuctionPen(diameter: 8).Vertical();
            });

            // 1.2 捕获可视化配置
            var visRegistry = new CaptureVisualRegistry();
            visuals.Build()(visRegistry);

            // 2. 转换为 Config
            var config = BlueprintInterpreter.ToConfig(bp);

            // 3. 映射到 Web Schema
            // 3. 映射到 Web Schema
            var webModel = WebMachineModelMapper.MapToWebModel(config, visRegistry.Model);

            // 4. 序列化
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(webModel, options);
            
            // Write to file for easier retrieval
            System.IO.File.WriteAllText("web_export.json", json);

            _output.WriteLine("JSON Output Start >>>");
            _output.WriteLine(json);
            _output.WriteLine("<<< JSON Output End");
        }

        [Fact]
        public void Generate_Web_Json_For_WinMachine_Full()
        {
            // 1. 构建全量 WinMachine 蓝图
            var bp = MachineBlueprint.Define("WinMachine_Full")
                .AddBoard("MotionCard_1", 0, b => b.UseSimulator()
                    .AddAxis(Axis_X, 0, a => a.WithRange(0, 800))
                    .AddAxis(Axis_Y1, 1, a => a.WithRange(0, 400))
                    .AddAxis(Axis_Y2, 2, a => a.WithRange(0, 400))
                    .AddAxis(Axis_Z1, 3, a => a.WithRange(0, 150).Vertical())
                    .AddAxis(Axis_Z2, 4, a => a.WithRange(0, 150).Vertical())
                    .AddAxis(Axis_R, 5, a => a.WithRange(0, 180))) // 180度翻转
                .AddBoard("IOCard_1", 1, b => b.UseSimulator()
                    .AddCylinder(Cyl_LoadPen1, 0, 0)
                    .AddCylinder(Cyl_UnloadPen1, 1, 1)
                    .AddCylinder(Cyl_LoadPen2, 2, 2)
                    .AddCylinder(Cyl_UnloadPen2, 3, 3)
                    .AddCylinder(Cyl_ScanPush, 4, 4)
                    .AddCylinder(Cyl_GripperClamp, 5, 5)
                    .AddCylinder(Cyl_GripperLift, 6, 6)
                    .AddCylinder(Cyl_ScanVac1, 7, 7)
                    .AddCylinder(Cyl_ScanVac2, 8, 8)
                    .AddCylinder(Cyl_ScanVac3, 9, 9)
                    .AddCylinder(Cyl_ScanVac4, 10, 10))
                .Mount("MachineBase", m => m
                    // Y Axis defined Trays (Ground level)
                    .Mount("TrayModule_Unload", t => t.LinkTo(Axis_Y1).WithOffset(y: -200, x: 0, z: 0)
                        .Mount("Tray_Unload_Plat", p => p.WithOffset(0, 0, 10))) 
                    .Mount("TrayModule_Load", t => t.LinkTo(Axis_Y2).WithOffset(y: 200, x: 0, z: 0)
                        .Mount("Tray_Load_Plat", p => p.WithOffset(0, 0, 10)))

                    // X Axis Beam (Elevated)
                    .Mount("Beam_X", x => x.LinkTo(Axis_X).WithOffset(z: 300)
                        // Z1 Module
                        .Mount("Head_Z1", z => z.LinkTo(Axis_Z1).WithOffset(x: -100)
                            .Mount("Pen_L1", p => p.LinkTo(Cyl_LoadPen1).WithOffset(x: -20))
                            .Mount("Pen_U1", p => p.LinkTo(Cyl_UnloadPen1).WithOffset(x: 20)))
                        // Z2 Module
                        .Mount("Head_Z2", z => z.LinkTo(Axis_Z2).WithOffset(x: 100)
                            .Mount("Pen_L2", p => p.LinkTo(Cyl_LoadPen2).WithOffset(x: -20))
                            .Mount("Pen_U2", p => p.LinkTo(Cyl_UnloadPen2).WithOffset(x: 20)))
                    )

                    // Scan Station (Fixed Ground position, but has moving parts)
                    .Mount("Scan_Station", s => s.WithOffset(x: 400, y: 0, z: 0)
                        .Mount("Scan_Pusher", p => p.LinkTo(Cyl_ScanPush) // Cylinder moves the seat
                            .Mount("Scan_Seat", seat => seat.WithOffset(0, 0, 50)
                                // Gripper Mechanism on the seat (Lifts and Rotates)
                                .Mount("Gripper_Lifter", l => l.LinkTo(Cyl_GripperLift)
                                    .Mount("Gripper_Rotator", r => r.LinkTo(Axis_R)
                                        .Mount("Gripper_Jaws", g => g.LinkTo(Cyl_GripperClamp))
                                    )
                                )
                                // Vacuum Positions on Seat
                                .Mount("Vac_1", v => v.LinkTo(Cyl_ScanVac1).WithOffset(x: -20, y: -20, z: 5))
                                .Mount("Vac_2", v => v.LinkTo(Cyl_ScanVac2).WithOffset(x: 20, y: -20, z: 5))
                                .Mount("Vac_3", v => v.LinkTo(Cyl_ScanVac3).WithOffset(x: -20, y: 20, z: 5))
                                .Mount("Vac_4", v => v.LinkTo(Cyl_ScanVac4).WithOffset(x: 20, y: 20, z: 5))
                            )
                        )
                    )
                );

            // 1.1 定义可视化 (Visual DSL)
            var visuals = Visuals.Define(v =>
            {
                // Axes
                v.For(Axis_X).AsLinearGuide(length: 1000, sliderWidth: 50).Horizontal();
                v.For(Axis_Y1).AsLinearGuide(length: 500, sliderWidth: 40).Horizontal();
                v.For(Axis_Y2).AsLinearGuide(length: 500, sliderWidth: 40).Horizontal();
                v.For(Axis_Z1).AsLinearGuide(length: 200, sliderWidth: 30).Vertical();
                v.For(Axis_Z2).AsLinearGuide(length: 200, sliderWidth: 30).Vertical();
                v.For(Axis_R).AsRotaryTable(radius: 40).WithPivot(0.5, 0.5);

                // Cylinders
                // Pens
                v.For(Cyl_LoadPen1).AsSuctionPen(diameter: 10).Vertical();
                v.For(Cyl_UnloadPen1).AsSuctionPen(diameter: 10).Vertical().Reversed(); // 假设反向
                v.For(Cyl_LoadPen2).AsSuctionPen(diameter: 10).Vertical();
                v.For(Cyl_UnloadPen2).AsSuctionPen(diameter: 10).Vertical().Reversed();

                // Scan & Gripper
                v.For(Cyl_ScanPush).AsSlideBlock(size: 60).Horizontal(); // Pushing cylinder
                v.For(Cyl_GripperLift).AsSlideBlock(size: 40).Vertical(); // Lifting cylinder
                v.For(Cyl_GripperClamp).AsGripper(open: 50, close: 10).Vertical(); // THe actual clamp

                // Vacuums (Small indicators)
                v.For(Cyl_ScanVac1).AsSuctionPen(diameter: 5).Vertical();
                v.For(Cyl_ScanVac2).AsSuctionPen(diameter: 5).Vertical();
                v.For(Cyl_ScanVac3).AsSuctionPen(diameter: 5).Vertical();
                v.For(Cyl_ScanVac4).AsSuctionPen(diameter: 5).Vertical();
            });

            // 1.2 Capture
            var visRegistry = new CaptureVisualRegistry();
            visuals.Build()(visRegistry);

            // 2. To Config
            var config = BlueprintInterpreter.ToConfig(bp);

            // 3. Map
            // 3. Map
            var webModel = WebMachineModelMapper.MapToWebModel(config, visRegistry.Model);
            webModel.MachineName = "WinMachine Full Configuration";

            // 4. Serialize
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(webModel, options);
            
            System.IO.File.WriteAllText("winmachine_full.json", json);

            _output.WriteLine("JSON Output (WinMachine Full) >>>");
            _output.WriteLine(json);
            _output.WriteLine("<<< JSON Output End");        }


    }
}
