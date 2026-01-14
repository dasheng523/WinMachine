using System;
using Common.Hardware;
using Devices.Sensors.Core;
using Devices.Sensors.Runners;
using LanguageExt;
using LanguageExt.Common; // for Error
using static LanguageExt.Prelude;

namespace Devices.Sensors.Serial;

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
        
        // 构建 DSL
        _readOp = from _0 in SerialOp.DiscardInBuffer()
                  from _1 in SerialOp.Write(options.StartCommand)
                  from val in SafeReadLine() // 使用安全的 Readline，确保 finally Write StopCommand
                  select CleanLine(val);
    }

    public string Name { get; }

    public Fin<string> Read()
    {
        return _runner.Run(_readOp, _options);
    }
    
    // 自定义一个 SafeReadLine，它模拟 try-finally 结构
    // 这种资源/状态清理模式在 Monad 中通常通过 Bracket 实现，但这里我们简单手写
    private SerialOp<string> SafeReadLine() => port =>
    {
        try
        {
            var line = port.ReadLine();
            return FinSucc(line);
        }
        catch (Exception ex)
        {
            return FinFail<string>(Error.New($"{Name} 读码超时/失败: {ex.Message}"));
        }
        finally
        {
            try { port.Write(_options.StopCommand); } catch { }
        }
    };

    private static string CleanLine(string? s) => (s ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}
