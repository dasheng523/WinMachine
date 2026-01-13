using System.IO.Ports;

namespace Devices.Sensors.Serial;

public sealed class SerialLineCommandOptions
{
    public string PortName { get; init; } = string.Empty;

    public int BaudRate { get; init; } = 115200;

    public Parity Parity { get; init; } = Parity.Even;

    public int DataBits { get; init; } = 8;

    public StopBits StopBits { get; init; } = StopBits.One;

    public string NewLine { get; init; } = "\r";

    public int ReadTimeoutMs { get; init; } = 3000;

    public string StartCommand { get; init; } = "LON\r";

    public string StopCommand { get; init; } = "LOFF\r";

    public bool RtsEnable { get; init; } = true;

    public bool DtrEnable { get; init; } = true;
}
