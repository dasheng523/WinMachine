using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Visualization;
using Machine.Framework.Visualization.SceneGraph;

namespace Machine.Framework.Interpreters.Visualization
{
    /// <summary>
    /// 负责将 Blueprint 的 Mount 定义转化为可视化的 SceneGraph
    /// </summary>
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
            // 1. 在 Blueprint 中查找根挂载点
            var rootDef = _mountPoints.FirstOrDefault(m => m.Name == rootName);
            if (rootDef == null)
            {
                // Fallback: 如果找不到定义的 Root，可能是一个虚拟 Root，创建一个空容器
                return new GroupNode { Name = rootName };
            }

            return BuildRecursive(rootDef);
        }

        private SceneNode BuildRecursive(MountPointDefinition def)
        {
            SceneNode node;
            VisualStyleDef? style = null;

            // Decision: 根据关联设备的类型决定创建什么类型的节点
            if (def.LinkedDevice is AxisID axisId)
            {
                _styleModel.Styles.TryGetValue(axisId.Name, out style);
                
                var axisNode = new AxisNode { Name = def.Name, BoundDeviceId = axisId };
                
                if (style != null)
                {
                    axisNode.IsRotary = style.Type == "RotaryTable";
                    axisNode.IsVertical = style.IsVertical;
                    if (style.Type == "LinearGuide")
                    {
                        axisNode.Length = (float)style.Param1;
                        axisNode.Width = (float)style.Param2;
                        // Map Height from style if set, otherwise default to Width
                        axisNode.Height = style.Height > 0 ? style.Height : axisNode.Width;
                    }
                    else if (style.Type == "RotaryTable")
                    {
                        axisNode.Width = (float)style.Param1; // Radius -> Width
                    }
                    else
                    {
                        // Generic/Default Axis Style
                        axisNode.Width = style.Width;
                        axisNode.Height = style.Height;
                    }
                }
                
                node = axisNode;
            }
            else if (def.LinkedDevice is CylinderID cylId)
            {
                _styleModel.Styles.TryGetValue(cylId.Name, out style);

                if (style != null && style.Type == "SlideBlock")
                {
                    // SlideBlock = Rail(fixed) + Carriage(moving) + (children mounted on output)
                    var length = (float)(style.Param1 > 0 ? style.Param1 : (style.Width > 0 ? style.Width : 120));
                    var thickness = style.Height > 0 ? style.Height : 32;
                    var isVertical = style.IsVertical;
                    var isReversed = style.IsReversed;

                    var rail = new SpriteNode { Name = def.Name + "_Rail" };
                    rail.PivotX = style.PivotX;
                    rail.PivotY = style.PivotY;
                    rail.Width = isVertical ? thickness : length;
                    rail.Height = isVertical ? length : thickness;
                    rail.CustomDraw = SpriteDraw.CreateSlideRailDraw(isVertical);

                    // Compute travel in pixels so carriage and mounted children really move.
                    var drawW = rail.Width;
                    var drawH = rail.Height;
                    var pad = MathF.Max(2, MathF.Min(drawW, drawH) * 0.08f);
                    var railW = (isVertical ? drawH : drawW) - pad * 2;
                    var railH = MathF.Max(6, (isVertical ? drawW : drawH) * 0.22f);
                    var blockW = MathF.Max(18, MathF.Min(railW * 0.30f, 60));
                    var travel = MathF.Max(0, railW - blockW);

                    var output = new StrokeNode
                    {
                        Name = def.Name,
                        BoundDeviceId = cylId,
                        IsVertical = isVertical,
                        IsReversed = isReversed,
                        Stroke = travel,
                        BaseX = isVertical ? 0 : -travel / 2f,
                        BaseY = isVertical ? -travel / 2f : 0
                    };

                    var carriage = new SpriteNode { Name = def.Name + "_Carriage" };
                    // Carriage centered on the stroke line
                    carriage.PivotX = 0.5f;
                    carriage.PivotY = 0.5f;
                    carriage.Width = blockW;
                    carriage.Height = MathF.Max(railH * 1.8f, (isVertical ? drawW : drawH) * 0.55f);
                    carriage.CustomDraw = SpriteDraw.CreateSlideCarriageDraw();
                    output.AddChild(carriage);

                    // Container node at mount offset
                    var container = new GroupNode { Name = def.Name + "_Slide" };
                    container.AddChild(rail);
                    container.AddChild(output);

                    node = container;

                    // Children should be mounted on the output end (carriage)
                    foreach (var childDef in def.Children)
                    {
                        var childNode = BuildRecursive(childDef);
                        output.AddChild(childNode);
                    }

                    // Apply mount offset to container
                    node.LocalX = (float)def.OffsetX;
                    node.LocalY = (float)def.OffsetY;
                    return node;
                }

                // Default cylinder = Sprite
                var sprite = new SpriteNode { Name = def.Name, BoundDeviceId = cylId };
                sprite.Color = System.Drawing.Color.Orange;

                if (style != null)
                {
                    sprite.Width = style.Width;
                    sprite.Height = style.Height;
                    sprite.PivotX = style.PivotX;
                    sprite.PivotY = style.PivotY;

                    if (style.Type == "Gripper") sprite.Color = System.Drawing.Color.MediumPurple;

                    if (style.Type != "Gripper" && style.Type != "SuctionPen" && style.Type != "Custom")
                        sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, style.IsVertical, style.IsReversed);
                }
                else
                {
                    sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, isVertical: false, isReversed: false);
                }

                node = sprite;
            }
            else
            {
                // 纯挂载点 (Group)
                node = new GroupNode { Name = def.Name };
            }

            // 应用 Blueprint 定义的初始偏移
            node.LocalX = (float)def.OffsetX;
            node.LocalY = (float)def.OffsetY;
            
            // 递归构建子节点
            foreach (var childDef in def.Children)
            {
                var childNode = BuildRecursive(childDef);
                node.AddChild(childNode);
            }

            return node;
        }
    }
}
