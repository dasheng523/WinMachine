using LanguageExt;

namespace MachineOrchestration.Core.Types;

/// <summary>气缸传感器配置（和类型）</summary>
public abstract record CylinderSensorConfig
{
    /// <summary>无传感器</summary>
    public sealed record None : CylinderSensorConfig
    {
        private None() { }
        public static readonly None Instance = new();
    }
    
    /// <summary>仅伸出传感器</summary>
    public sealed record ExtendOnly(SensorPort ExtendSensorPort) : CylinderSensorConfig;
    
    /// <summary>伸出和缩回传感器</summary>
    public sealed record Both(
        SensorPort ExtendSensorPort,
        SensorPort RetractSensorPort) : CylinderSensorConfig;
    
    private CylinderSensorConfig() { }
}

/// <summary>夹爪传感器配置</summary>
public sealed record GripperSensorConfig(
    Option<SensorPort> ClosedSensorPort,
    Option<SensorPort> OpenedSensorPort);

/// <summary>吸气装置传感器配置</summary>
public sealed record SuctionSensorConfig(SensorPort VacuumSensorPort);
