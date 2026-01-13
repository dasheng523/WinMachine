using System.IO.Ports;

namespace Devices.Sensors.Modbus;

public sealed class ModbusRtuConnectionOptions
{
    public string PortName { get; init; } = string.Empty;

    public int BaudRate { get; init; } = 115200;

    public Parity Parity { get; init; } = Parity.None;

    public int DataBits { get; init; } = 8;

    public StopBits StopBits { get; init; } = StopBits.One;

    public int ReadTimeoutMs { get; init; } = 500;

    public int WriteTimeoutMs { get; init; } = 500;
}
