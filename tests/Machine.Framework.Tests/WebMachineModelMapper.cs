using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Visualization;

namespace Machine.Framework.Tests.WebParams
{
    // Common Mapper to be shared across test files
    public static class WebMachineModelMapper
    {
        public static WebMachineModel MapToWebModel(MachineConfig config, VisualDefinitionModel visuals)
        {
            var devices = new Dictionary<string, WebDeviceInfo>();
            
            VisualStyleDef? GetStyle(string id) => visuals.Styles.TryGetValue(id, out var s) ? s : null;

            string ToCamelCase(string str)
            {
                if (string.IsNullOrEmpty(str) || char.IsLower(str[0])) return str;
                return char.ToLower(str[0]) + str.Substring(1);
            }

            Dictionary<string, object?> MergeMeta(object? baseMeta, VisualStyleDef? style, object? ioMeta = null)
            {
                var dict = new Dictionary<string, object?>();

                if (baseMeta != null)
                {
                    foreach (var prop in baseMeta.GetType().GetProperties())
                        dict[ToCamelCase(prop.Name)] = prop.GetValue(baseMeta);
                }

                if (ioMeta != null)
                {
                     foreach (var prop in ioMeta.GetType().GetProperties())
                        dict[ToCamelCase(prop.Name)] = prop.GetValue(ioMeta);
                }

                if (style != null)
                {
                    dict["width"] = style.Width;
                    dict["height"] = style.Height;
                    dict["pivotX"] = style.PivotX;
                    dict["pivotY"] = style.PivotY;
                    dict["isVertical"] = style.IsVertical;
                    dict["isReversed"] = style.IsReversed; 
                    
                    switch (style.Type)
                    {
                        case "Gripper":
                            dict["openWidth"] = style.Param1;
                            dict["closeWidth"] = style.Param2;
                            break;
                        case "RotaryTable":
                            dict["radius"] = style.Param1;
                            break;
                        case "SuctionPen":
                            dict["diameter"] = style.Param1;
                            break;
                        case "LinearGuide":
                            dict["length"] = style.Param1;
                            dict["sliderWidth"] = style.Param2;
                            break;
                        case "SlideBlock":
                             if (style.Param1 > 0) dict["size"] = style.Param1;
                             break;
                        default:
                            if (style.Param1 != 0) dict["param1"] = style.Param1;
                            if (style.Param2 != 0) dict["param2"] = style.Param2;
                            break;
                    }
                }
                else
                {
                    dict["isGeneric"] = true;
                }

                return dict;
            }

            foreach (var axis in config.AxisConfigs)
            {
                var style = GetStyle(axis.Key);
                
                int? channel = null;
                string? boardName = null;
                foreach(var b in config.BoardConfigs) 
                {
                    if (b.AxisMappings.TryGetValue(axis.Key, out var ch)) 
                    {
                        channel = ch;
                        boardName = b.Name;
                        break;
                    }
                }

                var ioInfo = new 
                { 
                    Channel = channel,
                    Board = boardName
                };

                devices[axis.Key] = new WebDeviceInfo 
                { 
                    Id = axis.Key, 
                    Type = style?.Type ?? "Axis", 
                    BaseType = "Axis",
                    Meta = MergeMeta(new { 
                        Min = axis.Value.SoftLimits?.Min ?? 0, 
                        Max = axis.Value.SoftLimits?.Max ?? 0 
                    }, style)
                };
            }

            foreach (var cyl in config.CylinderConfigs)
            {
                var style = GetStyle(cyl.Key);
                var cVal = cyl.Value;
                
                var ioInfo = new 
                { 
                    IoOut = cVal.OutputPort,
                    IoInExtended = cVal.ExtendedSensorPort,
                    IoInRetracted = cVal.RetractedSensorPort
                };

                devices[cyl.Key] = new WebDeviceInfo 
                { 
                    Id = cyl.Key, 
                    Type = style?.Type ?? "Cylinder",
                    BaseType = "Cylinder",
                    Meta = MergeMeta(new { cVal.MoveTime }, style, ioInfo)
                };
            }

            var rootDefs = config.MountPoints.Where(m => string.IsNullOrEmpty(m.ParentName)).ToList();
            
            var sceneRoot = new WebSceneNode 
            { 
                Name = "SceneRoot", 
                Children = rootDefs.Select(r => BuildNodeRecursive(r, config.MountPoints, devices)).ToList()
            };

            return new WebMachineModel 
            { 
                MachineName = "WinMachine Export",
                SchemaVersion = "1.0",
                Scene = sceneRoot,
                DeviceRegistry = devices.Values.ToList()
            };
        }

        private static WebSceneNode BuildNodeRecursive(
            MountPointDefinition def, 
            List<MountPointDefinition> allDefs, 
            Dictionary<string, WebDeviceInfo> deviceMap)
        {
            var node = new WebSceneNode
            {
                Name = def.Name,
                Offset = new WebVector3 { X = def.OffsetX, Y = def.OffsetY, Z = def.OffsetZ }
            };

            if (def.LinkedDevice != null)
            {
                string devId = def.LinkedDevice switch 
                {
                    Machine.Framework.Core.Primitives.AxisID a => a.Name,
                    Machine.Framework.Core.Primitives.CylinderID c => c.Name,
                    _ => def.LinkedDevice?.ToString() ?? ""
                };
                
                node.LinkedDeviceId = devId;

                if (deviceMap.TryGetValue(devId, out var info))
                {
                    node.NodeType = info.Type;
                }
            }

            if (def.Children != null && def.Children.Any())
            {
                node.Children = def.Children.Select(c => BuildNodeRecursive(c, allDefs, deviceMap)).ToList();
            }

            return node;
        }
    }

    public class WebMachineModel
    {
        public string MachineName { get; set; } = "";
        public string? SchemaVersion { get; set; }
        public WebSceneNode? Scene { get; set; }
        public List<WebDeviceInfo> DeviceRegistry { get; set; } = new();
    }

    public class WebSceneNode
    {
        public string Name { get; set; } = "";
        public string NodeType { get; set; } = "Group";
        public string? LinkedDeviceId { get; set; }
        public WebVector3? Offset { get; set; }
        public List<WebSceneNode> Children { get; set; } = new();
    }

    public class WebVector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class WebDeviceInfo
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string BaseType { get; set; } = "";
        public object Meta { get; set; } = new();
    }
}
