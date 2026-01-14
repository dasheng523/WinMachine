using System;
using Common.Core;
using Common.Hardware;
using Devices.Sensors.Core;
using Devices.Sensors.Runners;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Devices.Sensors.Modbus;

public sealed class ModbusCoilLevelSensor : ISensor<Level>
{
    private readonly ModbusRunner _runner;
    private readonly ModbusRtuConnectionOptions _conn;
    private readonly byte _slaveId;
    private readonly ushort _address;

    public ModbusCoilLevelSensor(
        string name,
        ModbusRunner runner,
        ModbusRtuConnectionOptions conn,
        byte slaveId,
        ushort address)
    {
        Name = name;
        _runner = runner;
        _conn = conn;
        _slaveId = slaveId;
        _address = address;
    }

    public string Name { get; }

    public Fin<Level> Read()
    {
        var op = ModbusOp.ReadCoils(_slaveId, _address, 1)
            .Map(bits => 
            {
                var on = bits != null && bits.Length > 0 && bits[0];
                return on ? Level.On : Level.Off;
            });

        return _runner.Run(op, _conn);
    }
}
