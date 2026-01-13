using Devices.Motion.Abstractions;

namespace WinMachine.Configuration;

public sealed class SingleStepOptions
{
    public LoadPen1PickOptions LoadPen1Pick { get; set; } = new();
}

public sealed class LoadPen1PickOptions
{
    public double XPos { get; set; }

    public double Y2Pos { get; set; }

    public double Z1DownPos { get; set; }

    public double Z1SafePos { get; set; }

    /// <summary>
    /// 真空 DO 的逻辑名（System.IoMap.Do 里配置）。
    /// </summary>
    public string VacuumDo { get; set; } = "Vacuum.LoadPen1";

    /// <summary>
    /// 吸取成功压力/真空 OK 的 Level 传感器逻辑名（System.SensorMap 或默认走 DI）。
    /// </summary>
    public string PressureOkSensor { get; set; } = "LoadPen1.PressureOk";

    public AxisSpeedOptions SpeedXY { get; set; } = new();

    public AxisSpeedOptions SpeedZ { get; set; } = new();

    public int MoveTimeoutMs { get; set; } = 15000;

    public int PollMs { get; set; } = 50;
}

public sealed class AxisSpeedOptions
{
    public double StartVel { get; set; } = 50;

    public double MaxVel { get; set; } = 200;

    public double AccTime { get; set; } = 0.2;

    public double DecTime { get; set; } = 0.2;

    public double SAcc { get; set; } = 50;

    public double SDec { get; set; } = 0.05;

    public AxisSpeed ToAxisSpeed() => new(StartVel, MaxVel, AccTime, DecTime, SAcc, SDec);
}
