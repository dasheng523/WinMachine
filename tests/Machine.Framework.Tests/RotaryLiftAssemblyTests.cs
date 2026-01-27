#pragma warning disable CS8618
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

using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace Machine.Framework.Tests.WebParams
{
    using Machine.Framework.Visualization;

    public class RotaryLiftAssemblyTests
    {
        private readonly ITestOutputHelper _output;

        public RotaryLiftAssemblyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // --- Complex Rotary Lift IDs ---
        // Left Assembly
        public static readonly CylinderID Cyl_R_Lift = new CylinderID("Cyl_R_Lift");
        public static readonly AxisID Axis_R_Table = new AxisID("Axis_R_Table");
        public static readonly CylinderID Cyl_Grips_Left = new CylinderID("Cyl_Grips_Left"); // 4 Grippers shared
        
        // Right Assembly
        public static readonly CylinderID Cyl_Lift_Right = new CylinderID("Cyl_Lift_Right");
        public static readonly AxisID Axis_Table_Right = new AxisID("Axis_Table_Right");
        public static readonly CylinderID Cyl_Grips_Right = new CylinderID("Cyl_Grips_Right"); // 4 Grippers shared

        // Middle Slide Module
        public static readonly CylinderID Cyl_Middle_Slide = new CylinderID("Cyl_Middle_Slide");
        public static readonly CylinderID Cyl_Mid_Vac1 = new CylinderID("Cyl_Mid_Vac1");
        public static readonly CylinderID Cyl_Mid_Vac2 = new CylinderID("Cyl_Mid_Vac2");
        public static readonly CylinderID Cyl_Mid_Vac3 = new CylinderID("Cyl_Mid_Vac3");
        public static readonly CylinderID Cyl_Mid_Vac4 = new CylinderID("Cyl_Mid_Vac4");

        [Fact]
        public void Generate_Web_Json_For_Complex_Rotary_Lift_Assembly()
        {
            // Scenario Description:
            // 1. Two separate assembly modules (Left and Right).
            //    - Each has a Lifter (Vertical SlideBlock)
            //    - A Rotary Table mounted on the Lifter
            //    - 4 Grippers on the Rotary Table (2 Left, 2 Right)
            // 2. A Middle Sliding Module
            //    - Slides Horizontally (Left/Right)
            //    - Carries 4 Vacuum Pens (2 Left Group, 2 Right Group)
            //    - The vacuum pens align with the grippers of the respective assembly when the slide moves.

            var bp = MachineBlueprint.Define("Complex_Rotary_Dual_Assembly")
                .AddBoard("SimCard", 0, b => b.UseSimulator()
                    // Left Components
                    .AddCylinder(Cyl_R_Lift, 0, 0)
                    .AddAxis(Axis_R_Table, 0, a => a.WithRange(0, 360))
                    .AddCylinder(Cyl_Grips_Left, 1, 1) // Shared ID for all 4 Left Grippers
                    
                    // Right Components
                    .AddCylinder(Cyl_Lift_Right, 10, 10)
                    .AddAxis(Axis_Table_Right, 1, a => a.WithRange(0, 360))
                    .AddCylinder(Cyl_Grips_Right, 11, 11) // Shared ID for all 4 Right Grippers
                    // Middle Module
                    .AddCylinder(Cyl_Middle_Slide, 20, 20)
                    .AddCylinder(Cyl_Mid_Vac1, 21, 21).AddCylinder(Cyl_Mid_Vac2, 22, 22)
                    .AddCylinder(Cyl_Mid_Vac3, 23, 23).AddCylinder(Cyl_Mid_Vac4, 24, 24))
                .Mount("MachineBase", m => m
                    // --- Middle Sliding Module ---
                    // Default at Right (Initial Offset +80)
                    // Vac3/4 Center at Local X=50 -> Global X = 80+50 = 130 (Aligns with Right Inner Grippers)
                    // Vac1/2 Center at Local X=-50 -> Global X = 80-50 = 30.
                    // When Slide Moved Left (Offset -80) -> Vac1/2 Global X = -80-50 = -130 (Aligns with Left Inner Grippers)
                    .Mount("Middle_Module", mid => mid.WithOffset(0, 0, 0)
                        .Mount("Slide_Push", s => s.LinkTo(Cyl_Middle_Slide).WithOffset(80, 0, 0) // Initial at Right
                            .Mount("Vac_Plate", p => p.WithOffset(0, 0, 50)
                                // Left Pair (for Left Module)
                                .Mount("Vac_Group_L", g => g.WithOffset(-50, 0, 0)
                                    .Mount("Vac1", v => v.LinkTo(Cyl_Mid_Vac1).WithOffset(0, -40, 0))
                                    .Mount("Vac2", v => v.LinkTo(Cyl_Mid_Vac2).WithOffset(0, 40, 0))
                                )
                                // Right Pair (for Right Module)
                                .Mount("Vac_Group_R", g => g.WithOffset(50, 0, 0)
                                    .Mount("Vac3", v => v.LinkTo(Cyl_Mid_Vac3).WithOffset(0, -40, 0))
                                    .Mount("Vac4", v => v.LinkTo(Cyl_Mid_Vac4).WithOffset(0, 40, 0))
                                )
                            )
                        )
                    )
                    // --- Left Assembly ---
                    .Mount("Assembly_Left", assembly => assembly.WithOffset(x: -250, y: 0, z: 0)
                        .Mount("Lifter_Column", l => l.LinkTo(Cyl_R_Lift).WithOffset(0, 0, 0)
                            .Mount("Rotary_Table", r => r.LinkTo(Axis_R_Table).WithOffset(0, 0, 100)
                                .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)
                                    .Mount("Grip_L1", grip => grip.LinkTo(Cyl_Grips_Left).WithOffset(0, -40, 0))
                                    .Mount("Grip_L2", grip => grip.LinkTo(Cyl_Grips_Left).WithOffset(0, 40, 0))
                                )
                                .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                    .Mount("Grip_R1", grip => grip.LinkTo(Cyl_Grips_Left).WithOffset(0, -40, 0))
                                    .Mount("Grip_R2", grip => grip.LinkTo(Cyl_Grips_Left).WithOffset(0, 40, 0))
                                )
                            )
                        )
                    )
                    // --- Right Assembly (Mirrored structure) ---
                    .Mount("Assembly_Right", assembly => assembly.WithOffset(x: 250, y: 0, z: 0)
                        .Mount("Lifter_Column", l => l.LinkTo(Cyl_Lift_Right).WithOffset(0, 0, 0)
                            .Mount("Rotary_Table", r => r.LinkTo(Axis_Table_Right).WithOffset(0, 0, 100)
                                .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)
                                    .Mount("Grip_L1", grip => grip.LinkTo(Cyl_Grips_Right).WithOffset(0, -40, 0))
                                    .Mount("Grip_L2", grip => grip.LinkTo(Cyl_Grips_Right).WithOffset(0, 40, 0))
                                )
                                .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                    .Mount("Grip_R1", grip => grip.LinkTo(Cyl_Grips_Right).WithOffset(0, -40, 0))
                                    .Mount("Grip_R2", grip => grip.LinkTo(Cyl_Grips_Right).WithOffset(0, 40, 0))
                                )
                            )
                        )
                    )
                );

            var visuals = Visuals.Define(v =>
            {
                // --- Left Styles ---
                v.For(Cyl_R_Lift).AsSlideBlock(size: 80).Vertical(); 
                v.For(Axis_R_Table).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
                v.For(Cyl_Grips_Left).AsGripper(open: 40, close: 10).Horizontal().Reversed();

                // --- Right Styles ---
                v.For(Cyl_Lift_Right).AsSlideBlock(size: 80).Vertical(); 
                v.For(Axis_Table_Right).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
                v.For(Cyl_Grips_Right).AsGripper(open: 40, close: 10).Horizontal();

                // --- Middle Styles ---
                v.For(Cyl_Middle_Slide).AsSlideBlock(size: 120).Horizontal(); 
                v.For(Cyl_Mid_Vac1).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac2).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac3).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac4).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Middle_Slide).AsSlideBlock(size: 120).Horizontal(); 
                v.For(Cyl_Mid_Vac1).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac2).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac3).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac4).AsSuctionPen(diameter: 8).Vertical();
            });

            // =========================================================================
            //  Flow DSL Logic 
            // =========================================================================
            // 1. Right Module: Close Grippers -> Lift Up -> Rotate 180 -> Lift Down -> Open Grippers
            // 2. Middle Slide: Move Left
            // 3. Left Module: Close Grippers -> Lift Up -> Rotate 180 -> Lift Down
            // 4. Middle Slide: Return
            
            var flow = 
                // --- Phase 1: Right Module Action ---
                from _1 in Name("右侧夹爪闭合(夹取)").Next(Cylinder(Cyl_Grips_Right).FireAndWait(false)) // False=Close
                from _2 in Name("右侧升起").Next(Cylinder(Cyl_Lift_Right).FireAndWait(false)) // False=Retracted=Up
                from _3 in Name("右侧旋转180").Next(Motion(Axis_Table_Right).MoveToAndWait(180))
                from _4 in Name("右侧降下").Next(Cylinder(Cyl_Lift_Right).FireAndWait(true)) // True=Extended=Down
                from _5 in Name("右侧夹爪松开(放料)").Next(Cylinder(Cyl_Grips_Right).FireAndWait(true)) // True=Open

                // --- Phase 2: Transfer Slide ---
                from _6 in Name("中间滑台向左").Next(Cylinder(Cyl_Middle_Slide).FireAndWait(true)) 

                // --- Phase 3: Left Module Action ---
                from _7 in Name("左侧夹爪闭合").Next(Cylinder(Cyl_Grips_Left).FireAndWait(false))
                from _8 in Name("左侧升起").Next(Cylinder(Cyl_R_Lift).FireAndWait(false))
                from _9 in Name("左侧旋转180").Next(Motion(Axis_R_Table).MoveToAndWait(180))
                from _10 in Name("左侧降下").Next(Cylinder(Cyl_R_Lift).FireAndWait(true))

                // --- Phase 4: Return ---
                from _11 in Name("中间滑台回原位").Next(Cylinder(Cyl_Middle_Slide).FireAndWait(false))
                
                select new Unit();

            // Capture & Export
            var visRegistry = new CaptureVisualRegistry();
            visuals.Build()(visRegistry);
            var config = BlueprintInterpreter.ToConfig(bp);
            var webModel = WebMachineModelMapper.MapToWebModel(config, visRegistry.Model);
            webModel.MachineName = "Complex Rotary Lift Assembly";

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(webModel, options);
            
            System.IO.File.WriteAllText("rotary_lift_assembly.json", json);

            _output.WriteLine("JSON Output (Rotary Lift) >>>");
            _output.WriteLine(json);
        }

        // ===================================
        // Telemetry Data Structure Definition
        // ===================================

        /// <summary>
        /// 遥测数据包：后端 -> 前端 (30Hz Push)
        /// </summary>
        public class TelemetryPacket
        {
            /// <summary>
            /// 逻辑时钟/时间戳 (Tick) - 用于消抖和排序
            /// </summary>
            [JsonPropertyName("t")]
            public long Tick { get; set; }

            /// <summary>
            /// 当前正在执行的步骤名称 (Context) - 用于UI高亮显示流程进度
            /// 必须对应 DSL 中的 Name("...") 业务这一层级，而非底层动作名，防止UI闪烁
            /// </summary>
            [JsonPropertyName("step")]
            public string StepName { get; set; }

            /// <summary>
            /// 视觉/动画位置 (Animation Targets)
            /// Key: DeviceID
            /// Value: 物理量 (mm/degree/width)
            /// 1. 统一使用物理单位，禁止使用 0-1 归一化值，前端根据静态配置的 Max/Min 渲染
            /// 2. 仅包含本帧相对于上一帧发生变化了的设备 (Dirty/Delta only)
            /// </summary>
            [JsonPropertyName("m")]
            public Dictionary<string, float> Motions { get; set; } = new();

            /// <summary>
            /// 真实IO/传感器状态 (Business Logic Truth)
            /// Key: SignalName (e.g., "Cyl_In", "Cyl_Out", "Pressure_1")
            /// Value: Boolean (Digital) or Float (Analog)
            /// 前端用于点亮虚拟面板上的指示灯
            /// </summary>
            [JsonPropertyName("io")]
            public Dictionary<string, object> IOs { get; set; } = new();

            /// <summary>
            /// 离散业务事件 (Discrete Events)
            /// 如：报警、提示、物流变更(Reparent)
            /// Include: "FlowStopped" (with Payload: { reason: "Complete" | "Error" | "UserStop" })
            /// </summary>
            [JsonPropertyName("e")]
            public List<TelemetryEvent> Events { get; set; } = new();
        }

        public class TelemetryEvent
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } // "Log", "Error", "Attach", "Detach", "Spawn", "FlowStopped"

            [JsonPropertyName("msg")]
            public string Message { get; set; } // Human readable message

            [JsonPropertyName("payload")]
            public object Payload { get; set; } // Extra data (e.g., childId, parentId)
        }
    }
}
