using System;
using LanguageExt;

namespace MachineOrchestration.ControlBoards.Types;

/// <summary>传感器读数</summary>
public abstract record SensorReading
{
    /// <summary>压力读数</summary>
    public sealed record Pressure(float Value, string Unit) : SensorReading;
    
    /// <summary>千分表读数</summary>
    public sealed record Micrometer(float Value) : SensorReading;
    
    /// <summary>扫码器读数</summary>
    public sealed record Scanner(string Data) : SensorReading;
    
    private SensorReading() { }
}

/// <summary>控制板状态</summary>
public sealed record ControlBoardState(
    DateTime Timestamp,
    bool IsInitialized,
    HashMap<MotorId, MotorState> MotorStates,
    HashMap<ActuatorId, ActuatorState> ActuatorStates,
    HashMap<SensorId, Option<SensorReading>> SensorReadings,
    HashMap<StateSensorId, bool> StateSensorStates)
{
    public static ControlBoardState Initial() => new(
        DateTime.UtcNow,
        false,
        HashMap<MotorId, MotorState>.Empty,
        HashMap<ActuatorId, ActuatorState>.Empty,
        HashMap<SensorId, Option<SensorReading>>.Empty,
        HashMap<StateSensorId, bool>.Empty);
}

/// <summary>电机状态</summary>
public sealed record MotorState(
    float CurrentPosition,
    float CurrentSpeed,
    bool IsMoving,
    bool IsHomed);

/// <summary>执行器状态</summary>
public sealed record ActuatorState(
    string CurrentState,
    bool IsActive);
