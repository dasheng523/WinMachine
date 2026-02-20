using LanguageExt;

namespace MachineOrchestration.Core.Types;

/// <summary>电机配置</summary>
public sealed record MotorConfig(
    float WorkingSpeed,
    HomingMode HomingMode,
    BoardConnection BoardConnection,
    LimitSensors LimitSensors);

/// <summary>回零模式</summary>
public enum HomingMode
{
    PositiveLimit,
    NegativeLimit,
    HomeSwitch
}

/// <summary>控制板连接</summary>
public sealed record BoardConnection(byte AxisNumber);

/// <summary>限位传感器</summary>
public sealed record LimitSensors(
    Option<SensorPort> PositiveLimit,
    Option<SensorPort> NegativeLimit);

/// <summary>执行器配置</summary>
public sealed record ActuatorConfig(
    ushort OutputPort,
    Option<StateSensorPorts> StateSensorPorts);

/// <summary>状态传感器端口配置</summary>
public abstract record StateSensorPorts
{
    public sealed record Cylinder(CylinderSensorConfig Config) : StateSensorPorts;
    public sealed record Gripper(GripperSensorConfig Config) : StateSensorPorts;
    public sealed record Suction(SuctionSensorConfig Config) : StateSensorPorts;
    
    private StateSensorPorts() { }
}

/// <summary>传感器配置</summary>
public sealed record SensorConfig(SensorConnection Connection);

/// <summary>传感器连接方式</summary>
public abstract record SensorConnection
{
    /// <summary>串口单传感器</summary>
    public sealed record SerialSingle(string Port, uint BaudRate) : SensorConnection;
    
    /// <summary>串口多传感器</summary>
    public sealed record SerialMultiple(string Port, uint BaudRate, byte Address) : SensorConnection;
    
    /// <summary>USB 连接</summary>
    public sealed record Usb(ushort VendorId, ushort ProductId) : SensorConnection;
    
    private SensorConnection() { }
}

/// <summary>零件配置（和类型）</summary>
public abstract record PartConfig
{
    public sealed record Motor(MotorConfig Config) : PartConfig;
    public sealed record Actuator(ActuatorConfig Config) : PartConfig;
    public sealed record Sensor(SensorConfig Config) : PartConfig;
    public sealed record Static : PartConfig
    {
        private Static() { }
        public static readonly Static Instance = new();
    }
    
    private PartConfig() { }
}
