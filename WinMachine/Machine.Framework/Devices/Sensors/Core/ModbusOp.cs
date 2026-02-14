using System;
using LanguageExt;
using LanguageExt.Common;
using Modbus.Device;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Core;

// ModbusOp<A> is essentially a Reader Monad: IModbusSerialMaster -> Fin<A>
public delegate Fin<A> ModbusOp<A>(IModbusSerialMaster master);

public static class ModbusOp
{
    // Return
    public static ModbusOp<A> Return<A>(A value) => _ => FinSucc(value);

    // Fail
    public static ModbusOp<A> Fail<A>(Error error) => _ => FinFail<A>(error);

    // Primitive: ReadHoldingRegisters
    public static ModbusOp<ushort[]> ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints) => master =>
    {
        try
        {
            var res = master.ReadHoldingRegisters(slaveId, startAddress, numberOfPoints);
            return FinSucc(res);
        }
        catch (Exception ex)
        {
            return FinFail<ushort[]>(Error.New(ex));
        }
    };

    // Primitive: WriteSingleRegister
    public static ModbusOp<Unit> WriteSingleRegister(byte slaveId, ushort registerAddress, ushort value) => master =>
    {
        try
        {
            master.WriteSingleRegister(slaveId, registerAddress, value);
            return FinSucc(unit);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(ex));
        }
    };

    // Primitive: WriteMultipleRegisters
    public static ModbusOp<Unit> WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] data) => master =>
    {
        try
        {
            master.WriteMultipleRegisters(slaveId, startAddress, data);
            return FinSucc(unit);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(ex));
        }
    };
    
    // Primitive: ReadCoils
    public static ModbusOp<bool[]> ReadCoils(byte slaveId, ushort startAddress, ushort numberOfPoints) => master =>
    {
        try
        {
            var res = master.ReadCoils(slaveId, startAddress, numberOfPoints);
            return FinSucc(res);
        }
        catch (Exception ex)
        {
            return FinFail<bool[]>(Error.New(ex));
        }
    };

    // Bind
    public static ModbusOp<B> Bind<A, B>(this ModbusOp<A> ma, Func<A, ModbusOp<B>> f) => master =>
        ma(master).Bind(a => f(a)(master));

    // Map
    public static ModbusOp<B> Map<A, B>(this ModbusOp<A> ma, Func<A, B> f) => master =>
        ma(master).Map(f);

    // LINQ Select
    public static ModbusOp<B> Select<A, B>(this ModbusOp<A> ma, Func<A, B> f) => Map(ma, f);

    // LINQ SelectMany
    public static ModbusOp<C> SelectMany<A, B, C>(
        this ModbusOp<A> ma,
        Func<A, ModbusOp<B>> bind,
        Func<A, B, C> project) => master =>
            ma(master).Bind(a => 
                bind(a)(master).Map(b => project(a, b)));
}


