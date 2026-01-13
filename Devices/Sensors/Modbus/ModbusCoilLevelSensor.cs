using System;
using Common.Core;
using Common.Hardware;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Devices.Sensors.Modbus;

public sealed class ModbusCoilLevelSensor : ISensor<Level>
{
    private readonly IModbusRtuMasterPool _pool;
    private readonly ModbusRtuConnectionOptions _conn;
    private readonly byte _slaveId;
    private readonly ushort _address;

    public ModbusCoilLevelSensor(
        string name,
        IModbusRtuMasterPool pool,
        ModbusRtuConnectionOptions conn,
        byte slaveId,
        ushort address)
    {
        Name = name;
        _pool = pool;
        _conn = conn;
        _slaveId = slaveId;
        _address = address;
    }

    public string Name { get; }

    public Fin<Level> Read()
    {
        try
        {
            return _pool.GetOrCreate(_conn).Map(master =>
            {
                // 读取 1 个 Coil
                var bits = master.ReadCoils(_slaveId, _address, 1);
                var on = bits is { Length: > 0 } && bits[0];
                return on ? Level.On : Level.Off;
            });
        }
        catch (Exception ex)
        {
            return FinFail<Level>(Error.New(new Exception($"ModbusCoil 读取失败: {Name} slave={_slaveId} addr={_address}, {ex.Message}", ex)));
        }
    }
}
