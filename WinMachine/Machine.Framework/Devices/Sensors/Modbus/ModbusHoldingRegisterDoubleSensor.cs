using Machine.Framework.Core.Hardware.Interfaces;
using System;
using Machine.Framework.Core.Hardware;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Modbus;

public sealed class ModbusHoldingRegisterDoubleSensor : ISensor<double>
{
    private readonly IModbusRtuMasterPool _pool;
    private readonly ModbusRtuConnectionOptions _conn;
    private readonly byte _slaveId;
    private readonly ushort _address;
    private readonly ushort _count;
    private readonly double _scale;
    private readonly double _offset;

    public ModbusHoldingRegisterDoubleSensor(
        string name,
        IModbusRtuMasterPool pool,
        ModbusRtuConnectionOptions conn,
        byte slaveId,
        ushort address,
        ushort count,
        double? scale,
        double? offset)
    {
        Name = name;
        _pool = pool;
        _conn = conn;
        _slaveId = slaveId;
        _address = address;
        _count = count;
        _scale = scale ?? 1.0;
        _offset = offset ?? 0.0;
    }

    public string Name { get; }

    public Fin<double> Read()
    {
        try
        {
            return _pool.GetOrCreate(_conn).Bind(master =>
            {
                var regs = master.ReadHoldingRegisters(_slaveId, _address, _count);
                if (regs is null || regs.Length < _count)
                {
                    return FinFail<double>(Error.New($"ModbusHoldingRegister 璇诲彇闀垮害涓嶈冻: {Name}"));
                }

                // 鍏煎鏃т唬鐮侊細Count=2 鏃?regs[0]=low, regs[1]=high锛岀粍鍚堟垚鏈夌鍙?Int32
                double raw = _count switch
                {
                    1 => regs[0],
                    2 => ToInt32LowHigh(regs[0], regs[1]),
                    _ => throw new NotSupportedException($"浠呮敮鎸?Count=1/2, 褰撳墠 Count={_count}")
                };

                var v = raw * _scale + _offset;
                return FinSucc(v);
            });
        }
        catch (Exception ex)
        {
            return FinFail<double>(Error.New(new Exception(
                $"ModbusHoldingRegister 璇诲彇澶辫触: {Name} slave={_slaveId} addr={_address} count={_count}, {ex.Message}", ex)));
        }
    }

    private static int ToInt32LowHigh(ushort low, ushort high)
    {
        uint combined = ((uint)high << 16) | low;
        return unchecked((int)combined);
    }
}


