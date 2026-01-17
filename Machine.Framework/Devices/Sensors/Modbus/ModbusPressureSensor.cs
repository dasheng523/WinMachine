using System;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Devices.Sensors.Core;
using Machine.Framework.Devices.Sensors.Runners;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Modbus;

public sealed class ModbusPressureSensor : ISensor<double>, IResettableSensor, IAlertLimitWritable
{
    private readonly ModbusRunner _runner;
    private readonly ModbusRtuConnectionOptions _conn;
    private readonly ModbusPressureSensorOptions _opt;

    public ModbusPressureSensor(
        string name,
        ModbusRunner runner,
        ModbusRtuConnectionOptions connection,
        ModbusPressureSensorOptions options)
    {
        Name = name;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _conn = connection ?? throw new ArgumentNullException(nameof(connection));
        _opt = options ?? throw new ArgumentNullException(nameof(options));

        if (_opt.Point <= 0) throw new ArgumentOutOfRangeException(nameof(options.Point));
    }

    public string Name { get; }

    public Fin<double> Read()
    {
        // 核心 DSL：读 -> 解析
        var op = from regs in ModbusOp.ReadHoldingRegisters(_opt.SlaveId, _opt.ReadAddress, 2)
                 let raw = ((uint)regs[1] << 16) | regs[0]
                 let signed = unchecked((int)raw)
                 select signed / _opt.Point;

        return _runner.Run(op, _conn);
    }

    public Fin<Unit> Reset()
    {
        var op = ModbusOp.WriteSingleRegister(_opt.SlaveId, _opt.ResetAddress, 4);
        return _runner.Run(op, _conn);
    }

    public Fin<Unit> WriteAlertLimit(double limitKg)
    {
        if (limitKg < 0) return FinFail<Unit>(Error.New($"{Name} 上限不能为负"));

        var op = from _ in ModbusOp.Return(unit) // start context
                 let limit = Convert.ToUInt32(limitKg * _opt.Point)
                 let low = (ushort)(limit & 0xFFFF)
                 let high = (ushort)((limit >> 16) & 0xFFFF)
                 from __ in ModbusOp.WriteMultipleRegisters(_opt.SlaveId, _opt.AlertLimitAddress, new[] { low, high })
                 select unit;

        return _runner.Run(op, _conn);
    }
}


