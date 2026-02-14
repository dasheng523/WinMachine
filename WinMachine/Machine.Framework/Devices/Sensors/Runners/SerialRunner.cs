using Machine.Framework.Core.Hardware.Interfaces;
using System;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Devices.Sensors.Core;
using Machine.Framework.Devices.Sensors.Serial;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Runners;

/// <summary>
/// 璐熻矗瑙ｉ噴鎵ц SerialOp銆?
/// 绠＄悊 SerialPortPool 鐨勮祫婧愮敵璇枫€侀攣瀹氥€侀厤缃?Config銆佹墽琛?IO銆佸紓甯稿鐞?(铏界劧 Op 鍐呴儴涔熸湁 try-catch锛屼絾 Runner 鎺у埗娴佺▼)銆?
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
                // 璁剧疆杩愯鏃跺弬鏁?
                port.NewLine = options.NewLine;
                port.ReadTimeout = options.ReadTimeoutMs;
                
                // 鎵ц绗竴娆?
                var result = op(port);

                // 绠€鍗曠殑閲嶈瘯閫昏緫 (濡傛灉闇€瑕?
                for (int i = 0; i < retries && result.IsFail; i++)
                {
                    // 鍙互鍦ㄨ繖閲屽姞 Delay
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


