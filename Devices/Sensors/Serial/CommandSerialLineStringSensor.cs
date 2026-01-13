using System;
using System.IO.Ports;
using Common.Hardware;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Devices.Sensors.Serial;

/// <summary>
/// 串口扫码器/读码器：每次 Read() 发送 StartCommand(LON)，读取一行后发送 StopCommand(LOFF)。
/// 行结束符默认 "\r"，与旧代码一致。
/// </summary>
public sealed class CommandSerialLineStringSensor : ISensor<string>
{
    private readonly ISerialPortPool _ports;
    private readonly SerialLineCommandOptions _options;

    public CommandSerialLineStringSensor(string name, ISerialPortPool ports, SerialLineCommandOptions options)
    {
        Name = name;
        _ports = ports;
        _options = options;
    }

    public string Name { get; }

    public Fin<string> Read()
    {
        try
        {
            var port = _ports.GetOrCreate(_options);
            var gate = _ports.GetLock(_options);

            lock (gate)
            {
                port.ReadTimeout = _options.ReadTimeoutMs;
                port.NewLine = _options.NewLine;

                port.DiscardInBuffer();

                // 必须按旧逻辑发 LON/LOFF
                port.Write(_options.StartCommand);

                try
                {
                    var line = port.ReadLine();
                    return FinSucc(CleanLine(line));
                }
                catch (TimeoutException)
                {
                    return FinFail<string>(Error.New($"{Name} 读码超时 (timeout={_options.ReadTimeoutMs}ms)"));
                }
                finally
                {
                    try
                    {
                        port.Write(_options.StopCommand);
                    }
                    catch
                    {
                        // ignore stop failures
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return FinFail<string>(Error.New(new Exception($"{Name} 串口读码失败: {ex.Message}", ex)));
        }
    }

    private static string CleanLine(string? s) => (s ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}
