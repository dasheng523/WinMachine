using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using Machine.Framework.Devices.Sensors.Core;

namespace Machine.Framework.Devices.Sensors.Serial;

public sealed class SerialPortPool : ISerialPortPool
{
    private sealed class Entry
    {
        public Entry(SerialPort port)
        {
            Port = port;
            Gate = new object();
        }

        public SerialPort Port { get; }
        public object Gate { get; }
    }

    private readonly ConcurrentDictionary<string, Entry> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 获取 (或创�? 一个原生的 System.IO.Ports.SerialPort 对象�?
    /// 该对象由连接池管理，请勿手动 Dispose�?
    /// </summary>
    public SerialPort GetOrCreate(SerialLineCommandOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PortName))
        {
            throw new ArgumentException("PortName 不能为空", nameof(options));
        }

        var key = BuildKey(options);
        var entry = _cache.GetOrAdd(key, _ => new Entry(CreatePort(options)));
        EnsureOpen(entry.Port);
        return entry.Port;
    }

    /// <summary>
    /// 获取 ITextLinePort 包装器，用于便捷�?ReadLine/WriteLine 操作�?
    /// </summary>
    public ITextLinePort GetOrCreateTextLinePort(SerialLineCommandOptions options)
    {
        var port = GetOrCreate(options);
        return new SerialPortTextLinePort(port);
    }

    /// <summary>
    /// 获取与该串口配置对应的线程锁对象�?
    /// 在进行连续的读写操作 (如先发命令再读响�? 时，应锁定该对象以保证原子性�?
    /// </summary>
    public object GetLock(SerialLineCommandOptions options)
    {
        var key = BuildKey(options);
        var entry = _cache.GetOrAdd(key, _ => new Entry(CreatePort(options)));
        return entry.Gate;
    }

    private static string BuildKey(SerialLineCommandOptions o) =>
        $"{o.PortName}|{o.BaudRate}|{(int)o.Parity}|{o.DataBits}|{(int)o.StopBits}|{o.NewLine}";

    private static SerialPort CreatePort(SerialLineCommandOptions o)
    {
        var port = new SerialPort
        {
            PortName = o.PortName,
            BaudRate = o.BaudRate,
            Parity = o.Parity,
            DataBits = o.DataBits,
            StopBits = o.StopBits,
            RtsEnable = o.RtsEnable,
            DtrEnable = o.DtrEnable,
            ReadTimeout = o.ReadTimeoutMs,
            NewLine = o.NewLine
        };

        return port;
    }

    private static void EnsureOpen(SerialPort port)
    {
        if (!port.IsOpen)
        {
            port.Open();
        }
    }

    public void Dispose()
    {
        foreach (var kv in _cache)
        {
            try
            {
                if (kv.Value.Port.IsOpen)
                {
                    kv.Value.Port.Close();
                }

                kv.Value.Port.Dispose();
            }
            catch
            {
                // ignore
            }
        }

        _cache.Clear();
    }
}


