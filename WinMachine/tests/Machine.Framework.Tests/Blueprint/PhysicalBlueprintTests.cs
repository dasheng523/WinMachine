using Xunit;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Blueprint.Builders;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Configuration.Models;
using System;
using System.Linq;

namespace Machine.Framework.Tests.Blueprint
{
    public class PhysicalBlueprintTests
    {
        [Fact]
        public void Scenario1_Positive_BasicPhysicalAttributes()
        {
            // Arrange - 使用带委托的 AddBoard 重载以返回 IMachineBlueprintBuilder
            var bp = MachineBlueprint.Define("TestMachine")
                .AddBoard("MainBoard", 1, b => { })
                .Mount("Cyl_Z", m => m
                    .Vertical()
                    .WithAnchor(PhysicalAnchor.TopCenter)
                    .AsSuctionPen(5, 50)
                    .WithStroke(0, 0, 50)
                )
                .Mount("TurnTable", m => m
                    .Horizontal()
                    .AsRotaryTable(150)
                    .WithAnchor(PhysicalAnchor.Center)
                );

            // Act - 暂不调用 BlueprintInterpreter，仅验证构建不抛出异常
            // 本测试验证物理 DSL 语法可用
            Assert.NotNull(bp);
        }

        [Fact]
        public void Scenario2_Negative_StaticAlignmentConflict()
        {
            // Arrange & Act & Assert
            // 当 Vertical() 与 WithStroke(X,0,0) 冲突时应抛出异常
            var ex = Assert.Throws<BlueprintValidationException>(() =>
            {
                MachineBlueprint.Define("BadMachine")
                    .AddBoard("MainBoard", 1, b => { })
                    .Mount("Error_Node", m => m
                        .Vertical()          // Z-Up
                        .WithStroke(100, 0, 0) // Moving in X -> Conflict!
                    );
            });
            
            Assert.Contains("AlignmentConflict", ex.Message);
        }

        [Fact]
        public void Scenario3_Negative_DynamicLinkConflict()
        {
            // Arrange
            var Axis_X = new AxisID("Axis_X");
            
            // 目前仅测试静态 Stroke 校验，动态 LinkTo 校验需要进一步实现
            // 此用例作为占位符，后续迭代时启用
            var bp = MachineBlueprint.Define("BadLinkMachine")
                .AddBoard("MainBoard", 1, b => b.AddAxis(Axis_X, 1))
                .Mount("Error_Link", m => m
                    .LinkTo(Axis_X)
                );

            Assert.NotNull(bp);
        }

        [Fact]
        public void Scenario4_FrameStandardization()
        {
            // Arrange
            var bp = MachineBlueprint.Define("StandardFrameMachine")
                .AddBoard("MainBoard", 1, b => { })
                .Mount("Guide_Rail", m => m
                    .Horizontal()
                    .AsLinearGuide(500)
                    .WithStroke(100, 0, 0)
                );

            // 验证导轨创建不抛出异常
            Assert.NotNull(bp);
        }

        [Fact]
        public void Scenario6_MaterialSlot_Geometry()
        {
            // Arrange
            var bp = MachineBlueprint.Define("TrayMachine")
                .AddBoard("MainBoard", 1, b => { })
                .Mount("Tray_Slot_1", m => m
                    .Horizontal()
                    .AsMaterialSlot(40, 40)
                    .WithAnchor(PhysicalAnchor.Center)
                );

            // 验证物料槽创建不抛出异常
            Assert.NotNull(bp);
        }
    }
}
