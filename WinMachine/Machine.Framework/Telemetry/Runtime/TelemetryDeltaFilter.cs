using System;
using System.Collections.Generic;

namespace Machine.Framework.Telemetry.Runtime;

public sealed class TelemetryDeltaFilter
{
    private readonly Dictionary<string, double> _last = new(StringComparer.Ordinal);

    public double Epsilon { get; }

    public TelemetryDeltaFilter(double epsilon)
    {
        if (epsilon <= 0) throw new ArgumentOutOfRangeException(nameof(epsilon));
        Epsilon = epsilon;
    }

    public Dictionary<string, double> Snapshot(Dictionary<string, double> current)
    {
        _last.Clear();
        foreach (var kv in current)
        {
            _last[kv.Key] = kv.Value;
        }

        return new Dictionary<string, double>(current, StringComparer.Ordinal);
    }

    public Dictionary<string, double> Delta(Dictionary<string, double> current)
    {
        var delta = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var kv in current)
        {
            if (_last.TryGetValue(kv.Key, out var lastVal))
            {
                if (Math.Abs(kv.Value - lastVal) <= Epsilon) continue;
            }

            delta[kv.Key] = kv.Value;
            _last[kv.Key] = kv.Value;
        }

        return delta;
    }
}
