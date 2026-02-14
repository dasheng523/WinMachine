using System.Collections.Generic;

namespace Machine.Framework.Telemetry.Schema;

public sealed class WebMachineModel
{
    public string MachineName { get; set; } = "";
    public string? SchemaVersion { get; set; }
    public WebSceneNode? Scene { get; set; }
    public List<WebDeviceInfo> DeviceRegistry { get; set; } = new();
}

public sealed class WebSceneNode
{
    public string Name { get; set; } = "";
    public string NodeType { get; set; } = "Group";
    public string? LinkedDeviceId { get; set; }
    public WebVector3? Offset { get; set; }
    public WebVector3? Rotation { get; set; }
    public WebVector3? Stroke { get; set; }
    
    // Physical Properties
    public string? PhysicalType { get; set; }
    public WebVector3? PhysicalSize { get; set; }
    public string? Anchor { get; set; }
    public bool? IsVertical { get; set; }
    public bool? IsInverted { get; set; }

    public List<WebSceneNode> Children { get; set; } = new();
}

public sealed class WebVector3
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

public sealed class WebDeviceInfo
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string BaseType { get; set; } = "";
    public object Meta { get; set; } = new();
}
