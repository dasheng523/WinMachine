using System.IO.Ports;

namespace Machine.Framework.Devices.Sensors.Modbus;

public sealed class ModbusRtuConnectionOptions
{
    /// <summary>
    /// 串口名称 (�?COM1)�?
    /// </summary>
    public string PortName { get; init; } = string.Empty;

    /// <summary>
    /// 波特�?(默认 115200)�?
    /// </summary>
    public int BaudRate { get; init; } = 115200;

    /// <summary>
    /// 奇偶校验 (默认 None)�?
    /// </summary>
    public Parity Parity { get; init; } = Parity.None;

    /// <summary>
    /// 数据�?(默认 8)�?
    /// </summary>
    public int DataBits { get; init; } = 8;

    /// <summary>
    /// 停止�?(默认 One)�?
    /// </summary>
    public StopBits StopBits { get; init; } = StopBits.One;

    /// <summary>
    /// 读取超时时间 (毫秒，默�?500ms)�?
    /// </summary>
    public int ReadTimeoutMs { get; init; } = 500;

    /// <summary>
    /// 写入超时时间 (毫秒，默�?500ms)�?
    /// </summary>
    public int WriteTimeoutMs { get; init; } = 500;
}


