using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Visualization;

namespace Machine.Framework.Telemetry.Schema;

public static class WebMachineModelMapper
{
    public static WebMachineModel MapToWebModel(MachineConfig config, VisualDefinitionModel visuals, string machineName)
    {
        var devices = new Dictionary<string, WebDeviceInfo>();

        VisualStyleDef? GetStyle(string id) => visuals.Styles.TryGetValue(id, out var s) ? s : null;

        static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0])) return str;
            return char.ToLower(str[0]) + str.Substring(1);
        }

        var allMountPoints = new List<MountPointDefinition>();
        void Collect(IEnumerable<MountPointDefinition> roots) 
        {
            foreach(var r in roots) {
                allMountPoints.Add(r);
                Collect(r.Children);
            }
        }
        Collect(config.MountPoints);

        PhysicalProperty? GetPhysical(string deviceId)
        {
            var def = allMountPoints.FirstOrDefault(m => {
                 string? lnk = m.LinkedDevice switch {
                      Machine.Framework.Core.Primitives.AxisID a => a.Name,
                      Machine.Framework.Core.Primitives.CylinderID c => c.Name,
                      _ => m.LinkedDevice?.ToString()
                 };
                 return lnk == deviceId;
            });
            return def?.Physical;
        }

        static Dictionary<string, object?> MergeMeta(object? baseMeta, VisualStyleDef? style, PhysicalProperty? physical, object? ioMeta = null)
        {
            var dict = new Dictionary<string, object?>();

            if (baseMeta != null)
            {
                foreach (var prop in baseMeta.GetType().GetProperties())
                {
                    dict[ToCamelCase(prop.Name)] = prop.GetValue(baseMeta);
                }
            }

            if (ioMeta != null)
            {
                foreach (var prop in ioMeta.GetType().GetProperties())
                {
                    dict[ToCamelCase(prop.Name)] = prop.GetValue(ioMeta);
                }
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
            else if (physical != null)
            {
                // Fallback: Infer visual style from PhysicalProperty
                dict["isVertical"] = physical.IsVertical;
                dict["isReversed"] = physical.IsInverted;
                // Physical size is usually (X,Y,Z), map to width/height if applicable?
                // For now, map type specific parameters
                
                switch (physical.Type)
                {
                    case PhysicalType.LinearGuide:
                        dict["length"] = physical.Param1; // Length
                        // physical.SizeY is usually width
                        dict["sliderWidth"] = 20; // Default or from SizeY?
                        break;
                    case PhysicalType.RotaryTable:
                        dict["radius"] = physical.Param1; // Radius
                        break;
                    case PhysicalType.SuctionPen:
                        dict["diameter"] = physical.Param1; // Diameter
                        break;
                    case PhysicalType.Gripper:
                        // No specific params in standard definition yet except maybe size
                        break;
                    case PhysicalType.MaterialSlot:
                         dict["width"] = physical.SizeX;
                         dict["height"] = physical.SizeY;
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
            var physical = GetPhysical(axis.Key);

            int? channel = null;
            string? boardName = null;
            foreach (var b in config.BoardConfigs)
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
                Type = style?.Type ?? physical?.Type.ToString() ?? "Axis",
                BaseType = "Axis",
                Meta = MergeMeta(
                    new { Min = axis.Value.SoftLimits?.Min ?? 0, Max = axis.Value.SoftLimits?.Max ?? 0 },
                    style,
                    physical,
                    ioInfo)
            };
        }

        foreach (var cyl in config.CylinderConfigs)
        {
            var style = GetStyle(cyl.Key);
            var physical = GetPhysical(cyl.Key);
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
                Type = style?.Type ?? physical?.Type.ToString() ?? "Cylinder",
                BaseType = "Cylinder",
                Meta = MergeMeta(new { cVal.MoveTime }, style, physical, ioInfo)
            };
        }

        var rootDefs = config.MountPoints.Where(m => string.IsNullOrEmpty(m.ParentName)).ToList();

        var sceneRoot = new WebSceneNode
        {
            Name = "SceneRoot",
            Children = rootDefs.Select(r => BuildNodeRecursive(r, devices)).ToList()
        };

        return new WebMachineModel
        {
            MachineName = machineName,
            SchemaVersion = "1.0",
            Scene = sceneRoot,
            DeviceRegistry = devices.Values.ToList()
        };
    }

    private static WebSceneNode BuildNodeRecursive(MountPointDefinition def, Dictionary<string, WebDeviceInfo> deviceMap)
    {
        var node = new WebSceneNode
        {
            Name = def.Name,
            Offset = new WebVector3 { X = def.OffsetX, Y = def.OffsetY, Z = def.OffsetZ },
            Rotation = new WebVector3 { X = def.RotationX, Y = def.RotationY, Z = def.RotationZ },
            Stroke = new WebVector3 { X = def.StrokeX, Y = def.StrokeY, Z = def.StrokeZ }
        };

        // Map Physical Properies
        if (def.Physical != null && def.Physical.Type != Core.Configuration.Models.PhysicalType.None)
        {
            node.PhysicalType = def.Physical.Type.ToString();
            node.PhysicalSize = new WebVector3 
            { 
                X = def.Physical.SizeX, 
                Y = def.Physical.SizeY, 
                Z = def.Physical.SizeZ 
            };
            node.Anchor = def.Physical.Anchor.ToString();
            
            // Only set boolean flags if true to keep JSON clean
            if (def.Physical.IsVertical) node.IsVertical = true;
            if (def.Physical.IsInverted) node.IsInverted = true;
        }

        // Cleanup empty vectors to keep JSON clean
        if (node.Rotation.X == 0 && node.Rotation.Y == 0 && node.Rotation.Z == 0) node.Rotation = null;
        if (node.Stroke.X == 0 && node.Stroke.Y == 0 && node.Stroke.Z == 0) node.Stroke = null;

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
            node.Children = def.Children.Select(c => BuildNodeRecursive(c, deviceMap)).ToList();
        }

        return node;
    }
}
