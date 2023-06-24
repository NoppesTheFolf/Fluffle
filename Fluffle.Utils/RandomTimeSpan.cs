using System;

namespace Noppes.Fluffle.Utils;

public static class RandomTimeSpan
{
    public static TimeSpan Between(TimeSpan inclusiveStart, TimeSpan inclusiveEnd)
    {
        if (inclusiveStart > inclusiveEnd)
            throw new ArgumentOutOfRangeException(nameof(inclusiveStart), "Start cannot be larger than end.");

        var ticks = Random.Shared.NextInt64(inclusiveStart.Ticks, inclusiveEnd.Ticks + 1);
        var timeSpan = TimeSpan.FromTicks(ticks);

        return timeSpan;
    }
}
