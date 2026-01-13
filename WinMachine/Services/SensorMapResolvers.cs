using System;
using System.Linq;
using Common.Core;
using Common.Hardware;
using Devices.Sensors.Modbus;
using Devices.Sensors.Serial;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using WinMachine.Configuration;
using static LanguageExt.Prelude;

namespace WinMachine.Services;

public sealed class SensorMapLevelResolver : IResolver<ISensor<Level>>
{
    private readonly IIoResolver _io;
    private readonly IOptions<SystemOptions> _options;
    private readonly IModbusRtuMasterPool _modbus;

    public SensorMapLevelResolver(IIoResolver io, IOptions<SystemOptions> options, IModbusRtuMasterPool modbus)
    {
        _io = io ?? throw new ArgumentNullException(nameof(io));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _modbus = modbus ?? throw new ArgumentNullException(nameof(modbus));
    }

    public Fin<ISensor<Level>> Resolve(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
        {
            return FinFail<ISensor<Level>>(Error.New("Sensor 名称不能为空"));
        }

        // 兼容：若 SensorMap 未声明该 key，则默认当作 DI-Level 传感器（历史行为）。
        var opt = TryGetSensorOptions(_options.Value, logicalName);
        if (opt.IsNone)
        {
            return _io.ResolveDi(logicalName)
                .Map(di => (ISensor<Level>)new DigitalInputSensor(logicalName, di));
        }

        return opt.Match(
            Some: s => ResolveByOptions(logicalName, s),
            None: () => FinFail<ISensor<Level>>(Error.New("SensorMap 查找失败")));
    }

    private Fin<ISensor<Level>> ResolveByOptions(string logicalName, SensorOptions s)
    {
        return s.Kind switch
        {
            SensorKind.DiLevel =>
                string.IsNullOrWhiteSpace(s.Di)
                    ? FinFail<ISensor<Level>>(Error.New($"Sensor={logicalName} Kind=DiLevel 但未配置 Di"))
                    : _io.ResolveDi(s.Di)
                        .Map(di => (ISensor<Level>)new DigitalInputSensor(logicalName, di)),

            SensorKind.ModbusCoil =>
                s.Modbus is null
                    ? FinFail<ISensor<Level>>(Error.New($"Sensor={logicalName} Kind=ModbusCoil 但未配置 Modbus"))
                    : FinSucc((ISensor<Level>)new ModbusCoilLevelSensor(
                        logicalName,
                        _modbus,
                        ToModbusConn(s.Modbus),
                        s.Modbus.SlaveId,
                        s.Modbus.Address)),

            _ => FinFail<ISensor<Level>>(Error.New($"Sensor={logicalName} Kind={s.Kind} 不能解析为 Level 传感器"))
        };
    }

    private static ModbusRtuConnectionOptions ToModbusConn(ModbusSensorOptions m) =>
        new()
        {
            PortName = m.PortName,
            BaudRate = m.BaudRate,
            // 旧项目压力传感器常用 Parity.None；扫码器才是 Even。
            Parity = System.IO.Ports.Parity.None,
            DataBits = 8,
            StopBits = System.IO.Ports.StopBits.One,
            ReadTimeoutMs = 500,
            WriteTimeoutMs = 500
        };

    private static Option<SensorOptions> TryGetSensorOptions(SystemOptions opt, string name)
    {
        var map = opt.SensorMap;
        if (map is null || map.Count == 0) return None;
        if (map.TryGetValue(name, out var hit)) return Some(hit);
        var v = map.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
        return v is null ? None : Some(v);
    }
}

public sealed class SensorMapDoubleResolver : IResolver<ISensor<double>>
{
    private readonly IOptions<SystemOptions> _options;
    private readonly IModbusRtuMasterPool _modbus;

    public SensorMapDoubleResolver(IOptions<SystemOptions> options, IModbusRtuMasterPool modbus)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _modbus = modbus ?? throw new ArgumentNullException(nameof(modbus));
    }

    public Fin<ISensor<double>> Resolve(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
        {
            return FinFail<ISensor<double>>(Error.New("Sensor 名称不能为空"));
        }

        var opt = TryGetSensorOptions(_options.Value, logicalName);
        if (opt.IsNone)
        {
            return FinFail<ISensor<double>>(Error.New($"未找到 DoubleSensor 映射: {logicalName}"));
        }

        return opt.Match(
            Some: s => s.Kind switch
            {
                SensorKind.ModbusHoldingRegister =>
                    s.Modbus is null
                        ? FinFail<ISensor<double>>(Error.New($"Sensor={logicalName} Kind=ModbusHoldingRegister 但未配置 Modbus"))
                        : FinSucc((ISensor<double>)new ModbusHoldingRegisterDoubleSensor(
                            logicalName,
                            _modbus,
                            ToModbusConn(s.Modbus),
                            s.Modbus.SlaveId,
                            s.Modbus.Address,
                            s.Modbus.Count,
                            s.Modbus.Scale,
                            s.Modbus.Offset)),

                _ => FinFail<ISensor<double>>(Error.New($"Sensor={logicalName} Kind={s.Kind} 不能解析为 double 传感器"))
            },
            None: () => FinFail<ISensor<double>>(Error.New("SensorMap 查找失败")));
    }

    private static ModbusRtuConnectionOptions ToModbusConn(ModbusSensorOptions m) =>
        new()
        {
            PortName = m.PortName,
            BaudRate = m.BaudRate,
            Parity = System.IO.Ports.Parity.None,
            DataBits = 8,
            StopBits = System.IO.Ports.StopBits.One,
            ReadTimeoutMs = 500,
            WriteTimeoutMs = 500
        };

    private static Option<SensorOptions> TryGetSensorOptions(SystemOptions opt, string name)
    {
        var map = opt.SensorMap;
        if (map is null || map.Count == 0) return None;
        if (map.TryGetValue(name, out var hit)) return Some(hit);
        var v = map.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
        return v is null ? None : Some(v);
    }
}

public sealed class SensorMapStringResolver : IResolver<ISensor<string>>
{
    private readonly IOptions<SystemOptions> _options;
    private readonly ISerialPortPool _serial;

    public SensorMapStringResolver(IOptions<SystemOptions> options, ISerialPortPool serial)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serial = serial ?? throw new ArgumentNullException(nameof(serial));
    }

    public Fin<ISensor<string>> Resolve(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
        {
            return FinFail<ISensor<string>>(Error.New("Sensor 名称不能为空"));
        }

        var opt = TryGetSensorOptions(_options.Value, logicalName);
        if (opt.IsNone)
        {
            return FinFail<ISensor<string>>(Error.New($"未找到 StringSensor 映射: {logicalName}"));
        }

        return opt.Match(
            Some: s => s.Kind switch
            {
                SensorKind.SerialLine =>
                    s.Serial is null
                        ? FinFail<ISensor<string>>(Error.New($"Sensor={logicalName} Kind=SerialLine 但未配置 Serial"))
                        : FinSucc((ISensor<string>)new CommandSerialLineStringSensor(
                            logicalName,
                            _serial,
                            ToSerialOptions(s.Serial))),

                _ => FinFail<ISensor<string>>(Error.New($"Sensor={logicalName} Kind={s.Kind} 不能解析为 string 传感器"))
            },
            None: () => FinFail<ISensor<string>>(Error.New("SensorMap 查找失败")));
    }

    private static SerialLineCommandOptions ToSerialOptions(SerialLineSensorOptions s) =>
        new()
        {
            PortName = s.PortName,
            BaudRate = s.BaudRate,
            // 旧扫码器：Parity.Even + NewLine=\r + LON/LOFF
            Parity = System.IO.Ports.Parity.Even,
            StopBits = System.IO.Ports.StopBits.One,
            DataBits = 8,
            NewLine = "\r",
            ReadTimeoutMs = 3000,
            StartCommand = "LON\r",
            StopCommand = "LOFF\r",
            RtsEnable = true,
            DtrEnable = true
        };

    private static Option<SensorOptions> TryGetSensorOptions(SystemOptions opt, string name)
    {
        var map = opt.SensorMap;
        if (map is null || map.Count == 0) return None;
        if (map.TryGetValue(name, out var hit)) return Some(hit);
        var v = map.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
        return v is null ? None : Some(v);
    }
}
