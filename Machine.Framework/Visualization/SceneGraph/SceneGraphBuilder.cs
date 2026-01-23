using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Visualization;
using Machine.Framework.Visualization.SceneGraph;

namespace Machine.Framework.Interpreters.Visualization
{
    public class SceneGraphBuilder
    {
        private readonly List<MountPointDefinition> _mountPoints;
        private readonly VisualDefinitionModel _styleModel;

        public SceneGraphBuilder(List<MountPointDefinition> mountPoints, VisualDefinitionModel styleModel)
        {
            _mountPoints = mountPoints ?? throw new ArgumentNullException(nameof(mountPoints));
            _styleModel = styleModel ?? throw new ArgumentNullException(nameof(styleModel));
        }

        public SceneNode Build(string rootName)
        {
            var rootDef = _mountPoints.FirstOrDefault(m => m.Name == rootName);
            if (rootDef == null) return new GroupNode { Name = rootName };
            return BuildRecursive(rootDef);
        }

        private SceneNode BuildRecursive(MountPointDefinition def)
        {
            SceneNode node;
            VisualStyleDef? style = null;

            if (def.LinkedDevice is AxisID axisId)
            {
                _styleModel.Styles.TryGetValue(axisId.Name, out style);
                var axisNode = new AxisNode { Name = def.Name, BoundDeviceId = axisId };
                bool isLinear = true;

                if (style != null)
                {
                    axisNode.IsRotary = style.Type == "RotaryTable";
                    axisNode.IsVertical = style.IsVertical;
                    isLinear = !axisNode.IsRotary;
                    if (style.Type == "LinearGuide")
                    {
                        axisNode.Length = (float)style.Param1;
                        axisNode.Width = (float)(style.Param2 > 0 ? style.Param2 : 40);
                        axisNode.Height = style.Height > 0 ? style.Height : axisNode.Width;
                    }
                    else if (style.Type == "RotaryTable")
                    {
                        axisNode.Width = (float)(style.Param1 > 0 ? style.Param1 : 40);
                    }
                    else
                    {
                        axisNode.Width = style.Width > 0 ? style.Width : 40;
                        axisNode.Height = style.Height > 0 ? style.Height : 40;
                    }
                }

                if (isLinear && style?.Type == "LinearGuide")
                {
                    var container = new GroupNode { Name = def.Name + "_LinearGroup" };
                    double min = 0, max = axisNode.Length > 0 ? axisNode.Length : 180;
                    var rail = new SpriteNode { Name = def.Name + "_Rail" };
                    rail.PivotX = 0.5f; rail.PivotY = 0.5f;
                    float railLength = (float)(max - min);
                    float railWidth = axisNode.Width * 0.6f;
                    rail.Width = axisNode.IsVertical ? railWidth : railLength;
                    rail.Height = axisNode.IsVertical ? railLength : railWidth;
                    if (axisNode.IsVertical) rail.LocalY = railLength / 2f + (float)min;
                    else rail.LocalX = railLength / 2f + (float)min;
                    rail.CustomDraw = SpriteDraw.CreateMotorRailDraw(axisNode.IsVertical, min, max);
                    container.AddChild(rail);
                    container.AddChild(axisNode);
                    node = container;
                }
                else
                {
                    node = axisNode;
                }
            }
            else if (def.LinkedDevice is CylinderID cylId)
            {
                _styleModel.Styles.TryGetValue(cylId.Name, out style);
                var sprite = new SpriteNode { Name = def.Name, BoundDeviceId = cylId };
                if (style != null)
                {
                    sprite.Width = style.Width; sprite.Height = style.Height;
                    sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                    if (style.Type == "Gripper") sprite.CustomDraw = SpriteDraw.CreateGripperDraw(() => sprite.CurrentValue, style.IsReversed);
                    else if (style.Type == "SuctionPen") sprite.CustomDraw = SpriteDraw.CreateSuctionPenDraw(() => sprite.CurrentValue);
                    else if (style.Type == "SlideBlock")
                    {
                        var length = (float)(style.Param1 > 0 ? style.Param1 : (style.Width > 0 ? style.Width : 120));
                        var thickness = style.Height > 0 ? style.Height : 32;
                        var isVertical = style.IsVertical;
                        var rail = new SpriteNode { Name = def.Name + "_Rail" };
                        rail.PivotX = 0; rail.PivotY = 0.5f;
                        rail.Width = isVertical ? thickness : length; rail.Height = isVertical ? length : thickness;
                        rail.CustomDraw = SpriteDraw.CreateSlideRailDraw(isVertical);
                        var railW = (isVertical ? rail.Height : rail.Width) - 4;
                        var blockW = MathF.Max(18, MathF.Min(railW * 0.30f, 60));
                        var travel = MathF.Max(0, railW - blockW);
                        var output = new StrokeNode { Name = def.Name, BoundDeviceId = cylId, IsVertical = isVertical, IsReversed = style.IsReversed, Stroke = travel };
                        var carriage = new SpriteNode { Name = def.Name + "_Carriage", PivotX = 0.5f, PivotY = 0.5f, Width = blockW, Height = MathF.Max(12, rail.Height * 0.8f) };
                        carriage.CustomDraw = SpriteDraw.CreateSlideCarriageDraw();
                        output.AddChild(carriage);
                        var slideGroup = new GroupNode { Name = def.Name + "_SlideGroup" };
                        slideGroup.AddChild(rail); slideGroup.AddChild(output);
                        node = slideGroup;
                    }
                    else if (style.Type != "Custom") sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, style.IsVertical, style.IsReversed);
                }
                else
                {
                    sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, false, false);
                }
                node = sprite;
            }
            else
            {
                node = new GroupNode { Name = def.Name };
            }

            node.LocalX = (float)def.OffsetX;
            node.LocalY = (float)def.OffsetY;

            if (def.Children != null)
            {
                foreach (var childDef in def.Children)
                {
                    var childNode = BuildRecursive(childDef);
                    
                    // 特殊逻辑：如果是混合节点，子项挂在运动部件上
                    if (node is GroupNode g)
                    {
                        var mover = g.Children.OfType<AxisNode>().FirstOrDefault() 
                                    ?? (SceneNode)g.Children.OfType<StrokeNode>().FirstOrDefault();
                        if (mover != null) mover.AddChild(childNode);
                        else g.AddChild(childNode);
                    }
                    else
                    {
                        node.AddChild(childNode);
                    }
                }
            }

            return node;
        }
    }
}