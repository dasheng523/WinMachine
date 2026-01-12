using Common.Core;
using LanguageExt;

namespace Common.Hardware;

public enum CylinderCommand
{
    Extend,
    Retract
}

public interface ISingleSolenoidCylinder
{
    string Name { get; }

    /// <summary>
    /// 单电磁阀输出。
    /// </summary>
    IDigitalOutput Valve { get; }

    /// <summary>
    /// 约定：Valve=On 对应的动作（Extend 或 Retract）。
    /// </summary>
    CylinderCommand OnMeans { get; }

    /// <summary>
    /// 可选：伸出到位（高有效）。
    /// </summary>
    Option<ISensor<Level>> ExtendedSensor { get; }

    /// <summary>
    /// 可选：缩回到位（高有效）。
    /// </summary>
    Option<ISensor<Level>> RetractedSensor { get; }

    /// <summary>
    /// 可选：健康/气压 OK（高有效）。
    /// </summary>
    Option<ISensor<Level>> HealthOkSensor { get; }

    /// <summary>
    /// 可选：压力表。
    /// </summary>
    Option<ISensor<double>> PressureSensor { get; }

    Fin<Unit> Command(CylinderCommand cmd);
}

public sealed class SingleSolenoidCylinder : ISingleSolenoidCylinder
{
    public SingleSolenoidCylinder(
        string name,
        IDigitalOutput valve,
        CylinderCommand onMeans,
        Option<ISensor<Level>> extendedSensor,
        Option<ISensor<Level>> retractedSensor,
        Option<ISensor<Level>> healthOkSensor,
        Option<ISensor<double>> pressureSensor)
    {
        Name = name;
        Valve = valve;
        OnMeans = onMeans;
        ExtendedSensor = extendedSensor;
        RetractedSensor = retractedSensor;
        HealthOkSensor = healthOkSensor;
        PressureSensor = pressureSensor;
    }

    public string Name { get; }

    public IDigitalOutput Valve { get; }

    public CylinderCommand OnMeans { get; }

    public Option<ISensor<Level>> ExtendedSensor { get; }

    public Option<ISensor<Level>> RetractedSensor { get; }

    public Option<ISensor<Level>> HealthOkSensor { get; }

    public Option<ISensor<double>> PressureSensor { get; }

    public Fin<Unit> Command(CylinderCommand cmd) =>
        Valve.Write(ToValveLevel(cmd));

    private Level ToValveLevel(CylinderCommand cmd) =>
        (cmd == OnMeans) ? Level.On : Level.Off;
}
