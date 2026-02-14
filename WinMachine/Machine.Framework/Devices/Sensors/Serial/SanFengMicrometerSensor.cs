using Machine.Framework.Core.Hardware.Interfaces;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Devices.Sensors.Core;
using Machine.Framework.Devices.Sensors.Runners;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Serial;

/// <summary>
/// 涓変赴/鍗冨垎琛ㄤ紶鎰熷櫒銆?
/// 浣跨敤 DSL 閲嶆柊瀹炵幇銆?
/// </summary>
public sealed class SanFengMicrometerSensor : ISensor<double>
{
    private static readonly Regex Normal = new(@"01A[+-]\d{4}\.\d{3}", RegexOptions.Compiled);
    private static readonly Regex Alarm = new(@"91\d{1}", RegexOptions.Compiled);

    private readonly SerialRunner _runner;
    private readonly SanFengMicrometerOptions _micrometer;
    private readonly SerialLineCommandOptions _serial;

    // 棰勫畾涔夌殑鍗忚閫昏緫 (Op)
    // 鍙互鍦ㄦ瀯閫犳椂鏍规嵁閰嶇疆鐢熸垚锛屼篃鍙互姣忔璋冪敤鐢熸垚
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

        // 鏋勫缓鍗忚鎻忚堪
        _readOp = BuildReadOp(_micrometer.TriggerCommand, Name);
    }

    public string Name { get; }

    public Fin<double> Read()
    {
        // 濮旀墭 Runner 鎵ц
        // 娉ㄦ剰锛氬師浠ｇ爜鏈夐噸璇曢€昏緫锛岀幇鍦ㄩ€氳繃 Runner 鐨?retries 鍙傛暟鏀寔锛堝鏋?Runner 鏀寔鐨勮瘽锛?
        // 鎴栬€呮垜浠彲浠ョ洿鎺ュ湪 DSL 閲屽啓寰幆銆?
        // 涓轰簡绠€鍗曪紝杩欓噷鍒╃敤 Runner 鐨?retry 鎴栬€呮垜浠彲浠ュ湪 Op 绾у埆鍐欓噸璇曠粍鍚堝瓙銆?
        
        // 鐢变簬鍘熼€昏緫姣旇緝鐗瑰畾锛圖iscard -> Write -> Read -> Parse -> if fail continue锛夛紝
        // 鏈€濂芥槸鍦?DSL 閲屾弿杩拌繖涓?Loop銆?
        
        // 璁╂垜浠畾涔変竴涓甫閲嶈瘯鐨?Op
        var op = RetryOp(_readOp, _micrometer.MaxAttempts);
        return _runner.Run(op, _serial);
    }
    
    // 缁勫悎瀛愶細閲嶈瘯
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
        // 杩欐槸涓€涓函鍑芥暟閫昏緫锛屼絾鏀惧湪 Bind 閾句腑鍙互鍐冲畾鍚庣画鏄?Success 杩樻槸 Fail
        // 杩欏氨鏄?Monad 鐨勯瓍鍔涳細鏍规嵁涓婁竴姝ョ粨鏋滃喅瀹氫笅涓€姝ユ帶鍒舵祦
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
                return FinFail<double>(Error.New($"{name} 鍗冨垎琛ㄦ姤璀︾爜: {alarm[alarm.Count - 1].Value}"));
            }

            return FinFail<double>(Error.New($"{name} 瑙ｆ瀽澶辫触/鏍煎紡閿欒: {line}"));
        };
    }
}


