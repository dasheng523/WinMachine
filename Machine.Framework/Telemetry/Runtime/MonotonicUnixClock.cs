using System;
using System.Threading;

namespace Machine.Framework.Telemetry.Runtime;

public sealed class MonotonicUnixClock
{
    private long _last;

    public long Now()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (true)
        {
            var last = Interlocked.Read(ref _last);
            var next = now <= last ? last + 1 : now;

            if (Interlocked.CompareExchange(ref _last, next, last) == last)
            {
                return next;
            }
        }
    }
}
