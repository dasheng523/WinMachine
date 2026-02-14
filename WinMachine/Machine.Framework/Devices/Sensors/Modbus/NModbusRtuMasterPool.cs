using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using LanguageExt;
using LanguageExt.Common;
using Modbus.Device;
using Modbus.IO;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Modbus;

public sealed class NModbusRtuMasterPool : IModbusRtuMasterPool, IDisposable
{
    private sealed class Entry
    {
        public Entry(SerialPort port, IModbusSerialMaster master)
        {
            Port = port;
            Master = master;
            Gate = new object();
        }

        public SerialPort Port { get; }
        public IModbusSerialMaster Master { get; }
        public object Gate { get; }
    }

    private readonly ConcurrentDictionary<string, Entry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public Fin<IModbusSerialMaster> GetOrCreate(ModbusRtuConnectionOptions options)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.PortName))
            {
                return FinFail<IModbusSerialMaster>(Error.New("Modbus PortName 不能为空"));
            }

            var key = BuildKey(options);
            var entry = _cache.GetOrAdd(key, _ => CreateEntry(options));
            EnsureOpen(entry.Port);
            return FinSucc(entry.Master);
        }
        catch (Exception ex)
        {
            return FinFail<IModbusSerialMaster>(Error.New(new Exception($"创建/获取 Modbus RTU Master 失败: {ex.Message}", ex)));
        }
    }

    public object GetLock(ModbusRtuConnectionOptions options)
    {
        var key = BuildKey(options);
        var entry = _cache.GetOrAdd(key, _ => CreateEntry(options));
        return entry.Gate;
    }

    private static string BuildKey(ModbusRtuConnectionOptions o) =>
        $"{o.PortName}|{o.BaudRate}|{(int)o.Parity}|{o.DataBits}|{(int)o.StopBits}";

    private static Entry CreateEntry(ModbusRtuConnectionOptions options)
    {
        var port = new SerialPort
        {
            PortName = options.PortName,
            BaudRate = options.BaudRate,
            Parity = options.Parity,
            DataBits = options.DataBits,
            StopBits = options.StopBits,
            ReadTimeout = options.ReadTimeoutMs,
            WriteTimeout = options.WriteTimeoutMs,
            RtsEnable = true,
            DtrEnable = true
        };

        EnsureOpen(port);

        // NModbus4 包暴露的命名空间�?Modbus.*；CreateRtu 需�?IStreamResource
        var stream = new SerialPortStreamResource(port);
        var master = ModbusSerialMaster.CreateRtu(stream);

        // 避免长期阻塞
        master.Transport.ReadTimeout = options.ReadTimeoutMs;
        master.Transport.WriteTimeout = options.WriteTimeoutMs;

        return new Entry(port, master);
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
                kv.Value.Master.Dispose();
            }
            catch
            {
                // ignore
            }

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


