using System;
using System.IO.Ports;

namespace Devices.Sensors.Serial;

public interface ISerialPortPool : IDisposable
{
    SerialPort GetOrCreate(SerialLineCommandOptions options);

    /// <summary>
    /// 获取该串口对应的互斥锁对象，用于保证一条读写序列原子。
    /// </summary>
    object GetLock(SerialLineCommandOptions options);
}
