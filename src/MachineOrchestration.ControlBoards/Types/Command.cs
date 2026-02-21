using System;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.ControlBoards.Types;

/// <summary>电机 ID（newtype 模式）</summary>
public readonly record struct MotorId(Guid Value)
{
    public static MotorId NewId() => new(Guid.NewGuid());
}

/// <summary>执行器 ID（newtype 模式）</summary>
public readonly record struct ActuatorId(Guid Value)
{
    public static ActuatorId NewId() => new(Guid.NewGuid());
}

/// <summary>传感器 ID（newtype 模式）</summary>
public readonly record struct SensorId(Guid Value)
{
    public static SensorId NewId() => new(Guid.NewGuid());
}

/// <summary>状态传感器 ID（newtype 模式）</summary>
public readonly record struct StateSensorId(Guid Value)
{
    public static StateSensorId NewId() => new(Guid.NewGuid());
}

/// <summary>控制板命令（代数数据类型）</summary>
public abstract record Command
{
    /// <summary>电机命令</summary>
    public sealed record Motor(MotorId MotorId, MotorAction Action) : Command;
    
    /// <summary>执行器命令</summary>
    public sealed record Actuator(ActuatorId ActuatorId, ActuatorAction Action) : Command;
    
    /// <summary>读取传感器</summary>
    public sealed record ReadSensor(SensorId SensorId) : Command;
    
    /// <summary>读取状态传感器</summary>
    public sealed record ReadStateSensor(StateSensorId StateSensorId) : Command;
    
    /// <summary>紧急停止</summary>
    public sealed record EmergencyStop : Command
    {
        private EmergencyStop() { }
        public static readonly EmergencyStop Instance = new();
    }
    
    private Command() { }
}
