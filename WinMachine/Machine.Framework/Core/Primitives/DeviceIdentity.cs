using System;

namespace Machine.Framework.Core.Primitives;

/// <summary>
/// 设备身份令牌基类
/// </summary>
public abstract record DeviceID(string Name)
{
    public override string ToString() => Name;
    public static implicit operator string(DeviceID id) => id.Name;
}

/// <summary>
/// 轴身分令牌
/// </summary>
public sealed record AxisID(string Name) : DeviceID(Name);

/// <summary>
/// 气缸身分令牌
/// </summary>
public sealed record CylinderID(string Name) : DeviceID(Name);

/// <summary>
/// 真空身分令牌
/// </summary>
public sealed record VacuumID(string Name) : DeviceID(Name);

/// <summary>
/// 传感器身分令牌
/// </summary>
public sealed record SensorID(string Name) : DeviceID(Name);
