using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Telemetry.Schema;
using Machine.Framework.Visualization;

namespace Machine.Framework.Tests.Telemetry
{
    public class PhysicalSchemaMapperTests
    {
        [Fact]
        public void MapToWebModel_ShouldIncludePhysicalProperties()
        {
            // Arrange
            var bp = MachineBlueprint.Define("PhysicalTestMachine")
                .AddBoard("MainBoard", 1, b => { })
                .Mount("SuctionPenNode", m => m
                    .Vertical()
                    .AsSuctionPen(10, 80)
                    .WithAnchor(PhysicalAnchor.TopCenter)
                )
                .Mount("LinearGuideNode", m => m
                    .Horizontal()
                    .AsLinearGuide(500)
                    .WithAnchor(PhysicalAnchor.StrokeStart)
                );

            var config = Machine.Framework.Interpreters.Configuration.BlueprintInterpreter.ToConfig(bp);
            var visuals = new VisualDefinitionModel(); // Empty visuals

            // Act
            var webModel = WebMachineModelMapper.MapToWebModel(config, visuals, "TestMachine");

            // Assert
            var root = webModel.Scene;
            root.Should().NotBeNull();
            
            // Find SuctionPenNode
            var suctionPen = FindNode(root, "SuctionPenNode");
            suctionPen.Should().NotBeNull();
            suctionPen.PhysicalType.Should().Be("SuctionPen");
            suctionPen.PhysicalSize.Should().BeEquivalentTo(new WebVector3 { X = 10, Y = 10, Z = 80 });
            suctionPen.Anchor.Should().Be("TopCenter");
            suctionPen.IsVertical.Should().BeTrue();

            // Find LinearGuideNode
            var guide = FindNode(root, "LinearGuideNode");
            guide.Should().NotBeNull();
            guide.PhysicalType.Should().Be("LinearGuide");
            guide.PhysicalSize.X.Should().Be(500);
            guide.Anchor.Should().Be("StrokeStart");
            guide.IsVertical.Should().BeNull(); // False should be null/omitted in JSON usually, but here mapped conditionally
        }

        private WebSceneNode FindNode(WebSceneNode node, string name)
        {
            if (node.Name == name) return node;
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    var found = FindNode(child, name);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}
