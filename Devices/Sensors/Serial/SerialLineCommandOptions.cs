using System.IO.Ports;

namespace Devices.Sensors.Serial;

public sealed class SerialLineCommandOptions
{
    /// <summary>
    /// 串口名称 (如 COM1)。
    /// </summary>
    public string PortName { get; init; } = string.Empty;

    /// <summary>
    /// 波特率 (默认 115200)。
    /// </summary>
    public int BaudRate { get; init; } = 115200;

    /// <summary>
    /// 奇偶校验 (默认 Even)。
    /// </summary>
    public Parity Parity { get; init; } = Parity.Even;

    /// <summary>
    /// 数据位 (默认 8)。
    /// </summary>
    public int DataBits { get; init; } = 8;

    /// <summary>
    /// 停止位 (默认 One)。
    /// </summary>
    public StopBits StopBits { get; init; } = StopBits.One;

    /// <summary>
    /// 行结束符 (默认 \r)。
    /// </summary>
    public string NewLine { get; init; } = "\r";

    /// <summary>
    /// 读取超时 (ms, 默认 3000)。
    /// </summary>
    public int ReadTimeoutMs { get; init; } = 3000;

    /// <summary>
    /// 开始读取命令 (如 LON)。
    /// </summary>
    public string StartCommand { get; init; } = "LON\r";

    /// <summary>
    /// 停止读取命令 (如 LOFF)。
    /// </summary>
    public string StopCommand { get; init; } = "LOFF\r";

    /// <summary>
    /// 启用 RTS (默认 true)。
    /// </summary>
    public bool RtsEnable { get; init; } = true;

    /// <summary>
    /// 启用 DTR (默认 true)。
    /// </summary>
    public bool DtrEnable { get; init; } = true;
}
