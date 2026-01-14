using System;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Devices.Sensors.Core;
using Machine.Framework.Devices.Sensors.Serial;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Runners;

/// <summary>
/// 负责解释执行 SerialOp�?
/// 管理 SerialPortPool 的资源申请、锁定、配�?Config、执�?IO、异常处�?(虽然 Op 内部也有 try-catch，但 Runner 控制流程)�?
/// </summary>
public sealed class SerialRunner
{
    private readonly ISerialPortPool _pool;

    public SerialRunner(ISerialPortPool pool)
    {
        _pool = pool;
    }

    public Fin<A> Run<A>(SerialOp<A> op, SerialLineCommandOptions options, int retries = 0)
    {
        try
        {
            var port = _pool.GetOrCreateTextLinePort(options);
            var gate = _pool.GetLock(options);

            lock (gate)
            {
                // 设置运行时参�?
                port.NewLine = options.NewLine;
                port.ReadTimeout = options.ReadTimeoutMs;
                
                // 执行第一�?
                var result = op(port);

                // 简单的重试逻辑 (如果需�?
                for (int i = 0; i < retries && result.IsFail; i++)
                {
                    // 可以在这里加 Delay
                   result = op(port);
                }
                
                return result;
            }
        }
        catch (Exception ex)
        {
            return FinFail<A>(Error.New(ex));
        }
    }
}


