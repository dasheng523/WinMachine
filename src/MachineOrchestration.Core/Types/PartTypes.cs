using System;
using System.Numerics;
using System.Text.Json.Serialization;
using LanguageExt;

namespace MachineOrchestration.Core.Types;

/// <summary>零件 ID（newtype 模式）</summary>
public readonly record struct PartId(Guid Value);

/// <summary>零件类型（和类型）</summary>
[JsonDerivedType(typeof(Motor), "motor")]
[JsonDerivedType(typeof(Actuator), "actuator")]
[JsonDerivedType(typeof(Sensor), "sensor")]
[JsonDerivedType(typeof(Static), "static")]
public abstract record PartType
{
    public sealed record Motor(MotorType Type) : PartType;
    public sealed record Actuator(ActuatorType Type) : PartType;
    public sealed record Sensor(SensorType Type) : PartType;
    public sealed record Static(StaticType Type) : PartType;
    
    private PartType() { }
}

/// <summary>电机类型</summary>
[JsonDerivedType(typeof(LinearScrew), "linearscrew")]
[JsonDerivedType(typeof(RotaryTable), "rotarytable")]
public abstract record MotorType
{
    /// <summary>丝杆滑块（滑块运动表达电机动作）</summary>
    public sealed record LinearScrew(
        float MaxSpeed,
        float StrokeLength) : MotorType;
    
    /// <summary>旋转座</summary>
    public sealed record RotaryTable(
        float MaxSpeed,
        float MaxAngle) : MotorType;
    
    private MotorType() { }
}

/// <summary>执行器类型（气缸、夹爪、吸气装置的统一抽象）</summary>
[JsonDerivedType(typeof(Cylinder), "cylinder")]
[JsonDerivedType(typeof(Gripper), "gripper")]
[JsonDerivedType(typeof(Suction), "suction")]
[JsonDerivedType(typeof(Indicator), "indicator")]
public abstract record ActuatorType
{
    /// <summary>气缸</summary>
    public sealed record Cylinder(
        float StrokeLength,
        CylinderSensorConfig SensorConfig) : ActuatorType;
    
    /// <summary>夹爪</summary>
    public sealed record Gripper(
        float MaxOpening,
        Option<GripperSensorConfig> SensorConfig) : ActuatorType;
    
    /// <summary>吸气装置</summary>
    public sealed record Suction(
        Option<SuctionSensorConfig> SensorConfig) : ActuatorType;
    
    /// <summary>指示灯</summary>
    public sealed record Indicator : ActuatorType
    {
        public Indicator() { }
        public static readonly Indicator Instance = new();
    }
    
    private ActuatorType() { }
}

/// <summary>传感器类型</summary>
[JsonDerivedType(typeof(Pressure), "pressure")]
[JsonDerivedType(typeof(Micrometer), "micrometer")]
[JsonDerivedType(typeof(Scanner), "scanner")]
public abstract record SensorType
{
    public sealed record Pressure(float Range, PressureUnit Unit) : SensorType;
    public sealed record Micrometer(float Resolution) : SensorType;
    public sealed record Scanner(ScannerProtocol Protocol) : SensorType;
    
    private SensorType() { }
}

/// <summary>压力单位</summary>
public enum PressureUnit { Pa, KPa, MPa, Bar, Psi }

/// <summary>扫码器协议</summary>
public enum ScannerProtocol { Serial, Usb, Ethernet }

/// <summary>静态零件类型</summary>
[JsonDerivedType(typeof(Shaft), "shaft")]
[JsonDerivedType(typeof(Bracket), "bracket")]
public abstract record StaticType
{
    public sealed record Shaft(float Length, float Diameter) : StaticType;
    public sealed record Bracket(Vector3 Dimensions) : StaticType;
    
    private StaticType() { }
}

/// <summary>传感器端口</summary>
public readonly record struct SensorPort(ushort PortNumber);

/// <summary>零件定义</summary>
public sealed record Part(
    PartId Id,
    string Name,
    PartType PartType,
    PartCategory Category,
    Vector3 PhysicalDimensions);
