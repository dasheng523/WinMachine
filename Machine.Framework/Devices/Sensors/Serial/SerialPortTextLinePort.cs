using System;
using System.IO.Ports;
using Machine.Framework.Devices.Sensors.Core;

namespace Machine.Framework.Devices.Sensors.Serial;

public sealed class SerialPortTextLinePort : ITextLinePort
{
    private readonly SerialPort _port;

    public SerialPortTextLinePort(SerialPort port)
    {
        _port = port ?? throw new ArgumentNullException(nameof(port));
    }

    public int ReadTimeout
    {
        get => _port.ReadTimeout;
        set => _port.ReadTimeout = value;
    }

    public string NewLine
    {
        get => _port.NewLine;
        set => _port.NewLine = value;
    }

    public void DiscardInBuffer() => _port.DiscardInBuffer();

    public void Write(string text) => _port.Write(text);

    public string ReadLine() => _port.ReadLine();
}


