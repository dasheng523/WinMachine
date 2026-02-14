using System;
using System.IO.Ports;
using Modbus.IO;

namespace Machine.Framework.Devices.Sensors.Modbus;

/// <summary>
/// NModbus4 需�?IStreamResource；该实现�?SerialPort 作为底层传输�?
/// </summary>
internal sealed class SerialPortStreamResource : IStreamResource
{
    private readonly SerialPort _port;

    public SerialPortStreamResource(SerialPort port)
    {
        _port = port ?? throw new ArgumentNullException(nameof(port));
    }

    public int InfiniteTimeout => SerialPort.InfiniteTimeout;

    public int ReadTimeout
    {
        get => _port.ReadTimeout;
        set => _port.ReadTimeout = value;
    }

    public int WriteTimeout
    {
        get => _port.WriteTimeout;
        set => _port.WriteTimeout = value;
    }

    public void DiscardInBuffer() => _port.DiscardInBuffer();

    public int Read(byte[] buffer, int offset, int count) => _port.Read(buffer, offset, count);

    public void Write(byte[] buffer, int offset, int count) => _port.Write(buffer, offset, count);

    public void Dispose()
    {
        // SerialPort 的生命周期由 Pool 管理，这里不关闭/释放�?
    }
}


