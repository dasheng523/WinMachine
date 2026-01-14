using System;
using Devices.Sensors.Core;
using Devices.Sensors.Modbus;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Devices.Sensors.Runners;

/// <summary>
/// 负责解释执行 ModbusOp。
/// </summary>
public sealed class ModbusRunner
{
    private readonly IModbusRtuMasterPool _pool;

    public ModbusRunner(IModbusRtuMasterPool pool)
    {
        _pool = pool;
    }

    public Fin<A> Run<A>(ModbusOp<A> op, ModbusRtuConnectionOptions options)
    {
        try
        {
            var gate = _pool.GetLock(options);
            lock (gate)
            {
                // 获取 Master
                return _pool.GetOrCreate(options).Bind(master => 
                {
                    // 设置超时等 (ModbusMaster通常在创建时绑定Stream, 但NModbus允许改Transport)
                    if (master.Transport != null)
                    {
                        master.Transport.ReadTimeout = options.ReadTimeoutMs;
                        master.Transport.WriteTimeout = options.WriteTimeoutMs;
                    }
                    
                    return op(master);
                });
            }
        }
        catch (Exception ex)
        {
            return FinFail<A>(Error.New(ex));
        }
    }
}
