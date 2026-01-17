using Machine.Framework.Core.Hardware.Interfaces;
using System;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Devices.Sensors.Core;
using Machine.Framework.Devices.Sensors.Runners;
using LanguageExt;
using LanguageExt.Common; // for Error
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Serial;

public sealed class CommandSerialLineStringSensor : ISensor<string>
{
    private readonly SerialRunner _runner;
    private readonly SerialLineCommandOptions _options;
    private readonly SerialOp<string> _readOp;

    public CommandSerialLineStringSensor(string name, SerialRunner runner, SerialLineCommandOptions options)
    {
        Name = name;
        _runner = runner;
        _options = options;
        
        // 鏋勫缓 DSL
        _readOp = from _0 in SerialOp.DiscardInBuffer()
                  from _1 in SerialOp.Write(options.StartCommand)
                  from val in SafeReadLine() // 浣跨敤瀹夊叏鐨?Readline锛岀‘淇?finally Write StopCommand
                  select CleanLine(val);
    }

    public string Name { get; }

    public Fin<string> Read()
    {
        return _runner.Run(_readOp, _options);
    }
    
    // 鑷畾涔変竴涓?SafeReadLine锛屽畠妯℃嫙 try-finally 缁撴瀯
    // 杩欑璧勬簮/鐘舵€佹竻鐞嗘ā寮忓湪 Monad 涓€氬父閫氳繃 Bracket 瀹炵幇锛屼絾杩欓噷鎴戜滑绠€鍗曟墜鍐?
    private SerialOp<string> SafeReadLine() => port =>
    {
        try
        {
            var line = port.ReadLine();
            return FinSucc(line);
        }
        catch (Exception ex)
        {
            return FinFail<string>(Error.New($"{Name} 璇荤爜瓒呮椂/澶辫触: {ex.Message}"));
        }
        finally
        {
            try { port.Write(_options.StopCommand); } catch { }
        }
    };

    private static string CleanLine(string? s) => (s ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}


