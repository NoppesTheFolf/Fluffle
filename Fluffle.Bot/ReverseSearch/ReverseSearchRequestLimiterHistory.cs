using Nito.AsyncEx;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Bot.ReverseSearch;

public record ReverseSearchRequestLimiterHistory
{
    public AsyncLock Lock { get; set; }

    public Queue<DateTime> Values { get; set; }

    public int Increment { get; set; }
}
