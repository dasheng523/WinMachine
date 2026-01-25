using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Visualization;
using Machine.Framework.Visualization.SceneGraph;

namespace Machine.Framework.Visualization.SceneGraph
{
    public class SceneGraphBuilder
    {
        private readonly IEnumerable<MountPointDefinition> _mounts;
        private readonly VisualDefinitionModel _visuals;

        public SceneGraphBuilder(IEnumerable<MountPointDefinition> mounts, VisualDefinitionModel visuals)
        {
            _mounts = mounts ?? Enumerable.Empty<MountPointDefinition>();
            _visuals = visuals ?? new VisualDefinitionModel();
        }

        public SceneNode Build(string rootName)
        {
            var rootDef = _mounts.FirstOrDefault(m => m.Name == rootName);
            if (rootDef == null)
            {
                // Just create a placeholder node
                return new GroupNode { Name = $"Missing_Root_{rootName}" };
            }

            return BuildRecursive(rootDef);
        }

        private SceneNode BuildRecursive(MountPointDefinition def)
        {
            SceneNode node = null;
            bool isHybrid = false;
            
            string devName = null;
            if (def.LinkedDevice is DeviceID did) devName = did.Name;
            else if (def.LinkedDevice is string s) devName = s;

            VisualStyleDef style = null;

            if (!string.IsNullOrEmpty(devName))
            {
                _visuals.Styles.TryGetValue(devName, out style);
            }

            if (style != null)
            {
                if (style.Type == "SlideBlock")
                {
                    node = BuildSlideBlockNode(style, devName);
                    isHybrid = true;
                }
                else if (style.Type == "LinearGuide")
                {
                    node = BuildLinearGuideNode(style, devName);
                    isHybrid = true;
                }
                else if (style.Type == "RotaryTable")
                {
                    var axisNode = new AxisNode { Name = devName, BoundDeviceId = new AxisID(devName) };
                    axisNode.IsRotary = true;
                    axisNode.Width = (float)(style.Param1 > 0 ? style.Param1 : 40);
                    node = axisNode;
                }
                else if (style.Type == "Gripper")
                {
                    var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                    sprite.Width = style.Width; sprite.Height = style.Height;
                    sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                    sprite.CustomDraw = SpriteDraw.CreateGripperDraw(() => sprite.CurrentValue, style.IsReversed);
                    node = sprite;
                }
                else if (style.Type == "SuctionPen")
                {
                    var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                    sprite.Width = style.Width; sprite.Height = style.Height;
                    sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                    sprite.CustomDraw = SpriteDraw.CreateSuctionPenDraw(() => sprite.CurrentValue);
                    node = sprite;
                }
                else
                {
                    // Generic fallback
                    var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                    sprite.Width = style.Width; sprite.Height = style.Height;
                    sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                    if (style.Type != "Custom")
                    {
                        sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(
                            () => sprite.CurrentValue, 
                            style.IsVertical, 
                            style.IsReversed);
                    }
                    node = sprite;
                }
            }
            else
            {
                // No style found
                if (!string.IsNullOrEmpty(devName))
                {
                     // Generic Sprite
                    var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName), Width = 60, Height = 24 };
                    // Default horizontal cylinder look
                    sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, false, false);
                    node = sprite;
                }
                else
                {
                    // Pure grouping node
                    node = new GroupNode { Name = def.Name };
                }
            }

            node.LocalX = (float)def.OffsetX;
            node.LocalY = (float)def.OffsetY;

            if (def.Children != null && def.Children.Any())
            {
                foreach (var childDef in def.Children)
                {
                    var childNode = BuildRecursive(childDef);
                    
                    if (isHybrid && node is GroupNode group)
                    {
                        var mover = FindMover(group, devName);
                        if (mover != null) mover.AddChild(childNode);
                        else group.AddChild(childNode);
                    }
                    else
                    {
                        node.AddChild(childNode);
                    }
                }
            }

            return node;
        }

        private SceneNode FindMover(GroupNode group, string devName)
        {
            var axis = group.Children.OfType<AxisNode>().FirstOrDefault(n => n.Name == devName);
            if (axis != null) return axis;

            var stroke = group.Children.OfType<StrokeNode>().FirstOrDefault(n => n.Name == devName);
            if (stroke != null) return stroke;

            return null;
        }

        private SceneNode BuildLinearGuideNode(VisualStyleDef style, string devName)
        {
            var axisNode = new AxisNode { Name = devName, BoundDeviceId = new AxisID(devName) };
            axisNode.IsVertical = style.IsVertical;
            axisNode.Length = (float)style.Param1; 
            axisNode.Width = (float)(style.Param2 > 0 ? style.Param2 : 40); 
            axisNode.Height = (float)(style.Height > 0 ? style.Height : axisNode.Width); 

            float min = 0, max = axisNode.Length > 0 ? axisNode.Length : 200;
            axisNode.TravelMin = min;
            axisNode.TravelMax = max;
            
            var rail = new SpriteNode { Name = devName + "_Rail" };
            float rLen = max - min;
            float rWid = axisNode.Width * 0.6f;
            rail.Width = axisNode.IsVertical ? rWid : rLen;
            rail.Height = axisNode.IsVertical ? rLen : rWid;
            
            // 使用 PivotX/Y=0 让导轨从起点开始绘制，与滑块坐标系一致
            // 滑块 LocalY=0 时在导轨起点，LocalY=Length 时在导轨终点
            if (axisNode.IsVertical)
            {
                rail.PivotX = 0.5f; rail.PivotY = 0f;
                rail.LocalX = 0; rail.LocalY = 0;
            }
            else
            {
                rail.PivotX = 0f; rail.PivotY = 0.5f;
                rail.LocalX = 0; rail.LocalY = 0;
            }

            rail.CustomDraw = SpriteDraw.CreateMotorRailDraw(axisNode.IsVertical, min, max);

            var slideGroup = new GroupNode { Name = devName + "_LinearGroup" };
            slideGroup.AddChild(rail);
            slideGroup.AddChild(axisNode);
            
            return slideGroup;
        }

        private SceneNode BuildSlideBlockNode(VisualStyleDef style, string devName)
        {
            var length = (float)(style.Param1 > 0 ? style.Param1 : (style.Width > 0 ? style.Width : 120));
            var thickness = style.Height > 0 ? style.Height : 32;
            var isVertical = style.IsVertical;

            var rail = new SpriteNode { Name = devName + "_Rail" };
            rail.Width = isVertical ? thickness : length;
            rail.Height = isVertical ? length : thickness;
            rail.CustomDraw = SpriteDraw.CreateSlideRailDraw(isVertical);
            
            // 使用 Pivot=0 让导轨从起点开始绘制，与滑块坐标系一致
            if (isVertical)
            {
                rail.PivotX = 0.5f; rail.PivotY = 0f;
                rail.LocalX = 0; rail.LocalY = 0;
            }
            else
            {
                rail.PivotX = 0f; rail.PivotY = 0.5f;
                rail.LocalX = 0; rail.LocalY = 0;
            }

            var pad = MathF.Max(2, MathF.Min(rail.Width, rail.Height) * 0.08f);
            var railLen = (isVertical ? rail.Height : rail.Width) - pad * 2;
            var railThick = MathF.Max(6, (isVertical ? rail.Width : rail.Height) * 0.22f);
            var blockW = MathF.Max(18, MathF.Min(railLen * 0.30f, 60));
            var travel = MathF.Max(0, railLen - blockW);

            // 滑块起始位置需要加上 padding 偏移，确保从导轨内部开始
            var output = new StrokeNode
            {
                Name = devName, BoundDeviceId = new CylinderID(devName),
                IsVertical = isVertical, IsReversed = style.IsReversed,
                Stroke = travel, 
                BaseX = isVertical ? 0 : pad + blockW / 2,
                BaseY = isVertical ? pad + blockW / 2 : 0
            };

            var carriage = new SpriteNode { Name = devName + "_Carriage" };
            carriage.PivotX = 0.5f; carriage.PivotY = 0.5f;
            carriage.Width = blockW; 
            carriage.Height = MathF.Max(railThick * 1.8f, (isVertical ? rail.Width : rail.Height) * 0.55f);
            carriage.CustomDraw = SpriteDraw.CreateSlideCarriageDraw();
            output.AddChild(carriage);

            var slideGroup = new GroupNode { Name = devName + "_Slide" };
            slideGroup.AddChild(rail); 
            slideGroup.AddChild(output);
            
            return slideGroup;
        }
    }
}