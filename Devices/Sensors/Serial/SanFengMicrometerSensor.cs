using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Common.Hardware;
using Devices.Sensors.Core;
using Devices.Sensors.Runners;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Devices.Sensors.Serial;

/// <summary>
/// 三丰/千分表传感器。
/// 使用 DSL 重新实现。
/// </summary>
public sealed class SanFengMicrometerSensor : ISensor<double>
{
    private static readonly Regex Normal = new(@"01A[+-]\d{4}\.\d{3}", RegexOptions.Compiled);
    private static readonly Regex Alarm = new(@"91\d{1}", RegexOptions.Compiled);

    private readonly SerialRunner _runner;
    private readonly SanFengMicrometerOptions _micrometer;
    private readonly SerialLineCommandOptions _serial;

    // 预定义的协议逻辑 (Op)
    // 可以在构造时根据配置生成，也可以每次调用生成
    private readonly SerialOp<double> _readOp;

    public SanFengMicrometerSensor(string name, SerialRunner runner, SanFengMicrometerOptions options)
    {
        Name = name;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _micrometer = options ?? throw new ArgumentNullException(nameof(options));

        _serial = new SerialLineCommandOptions
        {
            PortName = options.PortName,
            BaudRate = options.BaudRate,
            Parity = System.IO.Ports.Parity.None,
            DataBits = 8,
            StopBits = System.IO.Ports.StopBits.One,
            NewLine = options.NewLine,
            ReadTimeoutMs = options.ReadTimeoutMs,
            RtsEnable = true,
            DtrEnable = true,
            StartCommand = string.Empty,
            StopCommand = string.Empty
        };

        // 构建协议描述
        _readOp = BuildReadOp(_micrometer.TriggerCommand, Name);
    }

    public string Name { get; }

    public Fin<double> Read()
    {
        // 委托 Runner 执行
        // 注意：原代码有重试逻辑，现在通过 Runner 的 retries 参数支持（如果 Runner 支持的话）
        // 或者我们可以直接在 DSL 里写循环。
        // 为了简单，这里利用 Runner 的 retry 或者我们可以在 Op 级别写重试组合子。
        
        // 由于原逻辑比较特定（Discard -> Write -> Read -> Parse -> if fail continue），
        // 最好是在 DSL 里描述这个 Loop。
        
        // 让我们定义一个带重试的 Op
        var op = RetryOp(_readOp, _micrometer.MaxAttempts);
        return _runner.Run(op, _serial);
    }
    
    // 组合子：重试
    private static SerialOp<A> RetryOp<A>(SerialOp<A> op, int attempts) => port =>
    {
        Fin<A> lastResult = FinFail<A>(Error.New("No attempts made"));
        for (int i = 0; i < attempts; i++)
        {
            lastResult = op(port);
            if (lastResult.IsSucc) return lastResult;
        }
        return lastResult; // return last failure
    };

    private static SerialOp<double> BuildReadOp(string trigger, string name)
    {
        return from _0 in SerialOp.DiscardInBuffer()
               from _1 in SerialOp.Write(trigger)
               from line in SerialOp.ReadLine()
               from value in ParseLine(line, name)
               select value;
    }

    private static SerialOp<double> ParseLine(string line, string name)
    {
        // 这是一个纯函数逻辑，但放在 Bind 链中可以决定后续是 Success 还是 Fail
        // 这就是 Monad 的魅力：根据上一步结果决定下一步控制流
        return _ =>
        {
            var match = Normal.Matches(line ?? string.Empty);
            if (match.Count > 0)
            {
                var valStr = match[match.Count - 1].Value.Substring(3); // remove 01A
                if (double.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                {
                    return FinSucc(v);
                }
            }

            var alarm = Alarm.Matches(line ?? string.Empty);
            if (alarm.Count > 0)
            {
                return FinFail<double>(Error.New($"{name} 千分表报警码: {alarm[alarm.Count - 1].Value}"));
            }

            return FinFail<double>(Error.New($"{name} 解析失败/格式错误: {line}"));
        };
    }
}
