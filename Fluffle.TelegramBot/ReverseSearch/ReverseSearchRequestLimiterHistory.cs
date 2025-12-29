using System;
using System.Collections.Generic;
using Nito.AsyncEx;

namespace Fluffle.TelegramBot.ReverseSearch;

public record ReverseSearchRequestLimiterHistory
{
    public AsyncLock Lock { get; set; }

    public Queue<DateTime> Values { get; set; }

    public int Increment { get; set; }
}
