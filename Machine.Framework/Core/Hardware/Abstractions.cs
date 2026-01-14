using Machine.Framework.Core.Core;
using LanguageExt;

namespace Machine.Framework.Core.Hardware;

public interface IAxis
{
    string Name { get; }

    Fin<double> GetCommandPos();

    Fin<double> GetEncoderPos();

    Fin<Unit> MoveAbs(double pos);

    Fin<Unit> Stop();
}

public interface IDigitalInput
{
    string Name { get; }

    /// <summary>
    /// 读取数字量输入（Off/On）�?
    /// 约定：高有效（On 表示有效）�?
    /// </summary>
    Fin<Level> Read();
}

public interface IDigitalOutput
{
    string Name { get; }

    /// <summary>
    /// 写数字量输出（Off/On）�?
    /// </summary>
    Fin<Unit> Write(Level level);
}

public interface ISensor<T>
{
    string Name { get; }

    Fin<T> Read();
}

/// <summary>
/// 传感器原始值来源（DI / 串口 / Modbus 等）�?
/// </summary>
public interface IRawSensor
{
    string Name { get; }

    Fin<object?> ReadRaw();
}

/// <summary>
/// 容错转换器：�?Raw 值转为业务类型�?
/// </summary>
public interface IValueCoercer
{
    Fin<T> Coerce<T>(object? raw);
}

public interface IResolver<T>
{
    Fin<T> Resolve(string logicalName);
}

public static class SensorExtensions
{
    /// <summary>
    /// 高有效：On => true�?
    /// </summary>
    public static Fin<bool> ReadActive(this ISensor<Level> sensor) =>
        sensor.Read().Map(l => l == Level.On);

    /// <summary>
    /// 高有效：On => true�?
    /// </summary>
    public static Fin<bool> ReadActive(this IDigitalInput input) =>
        input.Read().Map(l => l == Level.On);
}


