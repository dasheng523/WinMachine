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
        public static readonly CylinderID Cyl_Grip_L1 = new CylinderID("Cyl_Grip_L1");
        public static readonly CylinderID Cyl_Grip_L2 = new CylinderID("Cyl_Grip_L2");
        public static readonly CylinderID Cyl_Grip_R1 = new CylinderID("Cyl_Grip_R1");
        public static readonly CylinderID Cyl_Grip_R2 = new CylinderID("Cyl_Grip_R2");

        // Right Assembly
        public static readonly CylinderID Cyl_Lift_Right = new CylinderID("Cyl_Lift_Right");
        public static readonly AxisID Axis_Table_Right = new AxisID("Axis_Table_Right");
        public static readonly CylinderID Cyl_Grip_Right_L1 = new CylinderID("Cyl_Grip_Right_L1");
        public static readonly CylinderID Cyl_Grip_Right_L2 = new CylinderID("Cyl_Grip_Right_L2");
        public static readonly CylinderID Cyl_Grip_Right_R1 = new CylinderID("Cyl_Grip_Right_R1");
        public static readonly CylinderID Cyl_Grip_Right_R2 = new CylinderID("Cyl_Grip_Right_R2");

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
                    .AddCylinder(Cyl_Grip_L1, 1, 1).AddCylinder(Cyl_Grip_L2, 2, 2)
                    .AddCylinder(Cyl_Grip_R1, 3, 3).AddCylinder(Cyl_Grip_R2, 4, 4)
                    // Right Components
                    .AddCylinder(Cyl_Lift_Right, 10, 10)
                    .AddAxis(Axis_Table_Right, 1, a => a.WithRange(0, 360))
                    .AddCylinder(Cyl_Grip_Right_L1, 11, 11).AddCylinder(Cyl_Grip_Right_L2, 12, 12)
                    .AddCylinder(Cyl_Grip_Right_R1, 13, 13).AddCylinder(Cyl_Grip_Right_R2, 14, 14)
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
                                    .Mount("Grip_L1", grip => grip.LinkTo(Cyl_Grip_L1).WithOffset(0, -40, 0))
                                    .Mount("Grip_L2", grip => grip.LinkTo(Cyl_Grip_L2).WithOffset(0, 40, 0))
                                )
                                .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                    .Mount("Grip_R1", grip => grip.LinkTo(Cyl_Grip_R1).WithOffset(0, -40, 0))
                                    .Mount("Grip_R2", grip => grip.LinkTo(Cyl_Grip_R2).WithOffset(0, 40, 0))
                                )
                            )
                        )
                    )
                    // --- Right Assembly (Mirrored structure) ---
                    .Mount("Assembly_Right", assembly => assembly.WithOffset(x: 250, y: 0, z: 0)
                        .Mount("Lifter_Column", l => l.LinkTo(Cyl_Lift_Right).WithOffset(0, 0, 0)
                            .Mount("Rotary_Table", r => r.LinkTo(Axis_Table_Right).WithOffset(0, 0, 100)
                                .Mount("Mount_Left", g => g.WithOffset(x: -120, y: 0, z: 0)
                                    .Mount("Grip_L1", grip => grip.LinkTo(Cyl_Grip_Right_L1).WithOffset(0, -40, 0))
                                    .Mount("Grip_L2", grip => grip.LinkTo(Cyl_Grip_Right_L2).WithOffset(0, 40, 0))
                                )
                                .Mount("Mount_Right", g => g.WithOffset(x: 120, y: 0, z: 0)
                                    .Mount("Grip_R1", grip => grip.LinkTo(Cyl_Grip_Right_R1).WithOffset(0, -40, 0))
                                    .Mount("Grip_R2", grip => grip.LinkTo(Cyl_Grip_Right_R2).WithOffset(0, 40, 0))
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
                v.For(Cyl_Grip_L1).AsGripper(open: 40, close: 10).Horizontal().Reversed();
                v.For(Cyl_Grip_L2).AsGripper(open: 40, close: 10).Horizontal().Reversed();
                v.For(Cyl_Grip_R1).AsGripper(open: 40, close: 10).Horizontal();
                v.For(Cyl_Grip_R2).AsGripper(open: 40, close: 10).Horizontal();

                // --- Right Styles ---
                v.For(Cyl_Lift_Right).AsSlideBlock(size: 80).Vertical(); 
                v.For(Axis_Table_Right).AsRotaryTable(radius: 100).WithPivot(0.5, 0.5);
                v.For(Cyl_Grip_Right_L1).AsGripper(open: 40, close: 10).Horizontal().Reversed();
                v.For(Cyl_Grip_Right_L2).AsGripper(open: 40, close: 10).Horizontal().Reversed();
                v.For(Cyl_Grip_Right_R1).AsGripper(open: 40, close: 10).Horizontal();
                v.For(Cyl_Grip_Right_R2).AsGripper(open: 40, close: 10).Horizontal();

                // --- Middle Styles ---
                v.For(Cyl_Middle_Slide).AsSlideBlock(size: 120).Horizontal(); 
                v.For(Cyl_Mid_Vac1).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac2).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac3).AsSuctionPen(diameter: 8).Vertical();
                v.For(Cyl_Mid_Vac4).AsSuctionPen(diameter: 8).Vertical();
            });

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
    }
}
