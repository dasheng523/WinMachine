using System.Globalization;
using Common.Core;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Common.Hardware;

public sealed class DefaultValueCoercer : IValueCoercer
{
    public Fin<T> Coerce<T>(object? raw)
    {
        var t = typeof(T);

        if (t == typeof(string))
        {
            var s = CoerceString(raw);
            return FinSucc((T)(object)s);
        }

        if (t == typeof(double))
        {
            return CoerceDouble(raw).Map(x => (T)(object)x);
        }

        if (t == typeof(Level))
        {
            return CoerceLevel(raw).Map(x => (T)(object)x);
        }

        return FinFail<T>(Error.New($"不支持的目标类型: {t.Name}"));
    }

    private static string CoerceString(object? raw) => raw switch
    {
        null => string.Empty,
        string s => s,
        Level l => l == Level.On ? "1" : "0",
        bool b => b ? "1" : "0",
        _ => Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty
    };

    private static Fin<double> CoerceDouble(object? raw)
    {
        if (raw is null)
        {
            return FinFail<double>(Error.New("raw 为空，无法转换为 double"));
        }

        if (raw is double d) return FinSucc(d);
        if (raw is float f) return FinSucc((double)f);
        if (raw is decimal m) return FinSucc((double)m);
        if (raw is int i) return FinSucc((double)i);
        if (raw is long l) return FinSucc((double)l);
        if (raw is short s) return FinSucc((double)s);
        if (raw is ushort us) return FinSucc((double)us);
        if (raw is uint ui) return FinSucc((double)ui);
        if (raw is ulong ul) return FinSucc((double)ul);

        if (raw is string str)
        {
            var trimmed = str.Trim();
            // 容错："12.3kPa" => 12.3（截取前导数字）
            var token = TakeLeadingNumber(trimmed);
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                || double.TryParse(token, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
            {
                return FinSucc(parsed);
            }

            return FinFail<double>(Error.New($"无法把字符串转换为 double: {str}"));
        }

        return FinFail<double>(Error.New($"无法把 {raw.GetType().Name} 转换为 double"));
    }

    private static Fin<Level> CoerceLevel(object? raw)
    {
        if (raw is null) return FinSucc(Level.Off);

        if (raw is Level l) return FinSucc(l);
        if (raw is bool b) return FinSucc(b ? Level.On : Level.Off);

        if (raw is double dd) return FinSucc(dd == 0 ? Level.Off : Level.On);
        if (raw is float f) return FinSucc(f == 0 ? Level.Off : Level.On);
        if (raw is decimal m) return FinSucc(m == 0 ? Level.Off : Level.On);

        if (raw is sbyte sb) return FinSucc(sb == 0 ? Level.Off : Level.On);
        if (raw is byte by) return FinSucc(by == 0 ? Level.Off : Level.On);
        if (raw is short s) return FinSucc(s == 0 ? Level.Off : Level.On);
        if (raw is ushort us) return FinSucc(us == 0 ? Level.Off : Level.On);
        if (raw is int i) return FinSucc(i == 0 ? Level.Off : Level.On);
        if (raw is uint ui) return FinSucc(ui == 0 ? Level.Off : Level.On);
        if (raw is long lo) return FinSucc(lo == 0 ? Level.Off : Level.On);
        if (raw is ulong ul) return FinSucc(ul == 0 ? Level.Off : Level.On);

        if (raw is string str)
        {
            var x = str.Trim();
            if (x.Length == 0) return FinSucc(Level.Off);

            if (IsOneOf(x, "1", "on", "true", "high", "h", "yes", "y")) return FinSucc(Level.On);
            if (IsOneOf(x, "0", "off", "false", "low", "l", "no", "n")) return FinSucc(Level.Off);

            // 容错：数字字符串
            if (double.TryParse(TakeLeadingNumber(x), NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                || double.TryParse(TakeLeadingNumber(x), NumberStyles.Float, CultureInfo.CurrentCulture, out d))
            {
                return FinSucc(d == 0 ? Level.Off : Level.On);
            }

            return FinFail<Level>(Error.New($"无法把字符串转换为 Level: {str}"));
        }

        return FinFail<Level>(Error.New($"无法把 {raw.GetType().Name} 转换为 Level"));
    }

    private static bool IsOneOf(string value, params string[] candidates) =>
        candidates.Any(c => string.Equals(value, c, StringComparison.OrdinalIgnoreCase));

    private static string TakeLeadingNumber(string input)
    {
        // 允许：+ - . 数字 e/E（科学计数），遇到第一个不符合就截断
        var span = input.AsSpan();
        var len = 0;
        for (var i = 0; i < span.Length; i++)
        {
            var ch = span[i];
            var ok = char.IsDigit(ch) || ch is '+' or '-' or '.' or 'e' or 'E';
            if (!ok) break;
            len++;
        }

        return len == 0 ? input : input[..len];
    }
}
