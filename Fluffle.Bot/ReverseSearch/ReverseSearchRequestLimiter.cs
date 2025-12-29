using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using Noppes.Fluffle.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.ReverseSearch;

public class ReverseSearchRequestLimiter
{
    private readonly AsyncLock _mutex = new();
    private readonly Dictionary<long, ReverseSearchRequestLimiterHistory> _chats = new();

    private readonly TimeSpan _expirationTime;
    private readonly TimeSpan _pressureTimeSpan;
    private readonly int _requestCount;
    private readonly int _saveEveryNthIncrement;

    private readonly BotContext _botContext;

    public ReverseSearchRequestLimiter(IOptions<BotConfiguration> options, BotContext botContext)
    {
        var rateLimiterConf = options.Value.ReverseSearch.RateLimiter;

        _expirationTime = TimeSpan.FromMinutes(rateLimiterConf.ExpirationTime);
        _pressureTimeSpan = TimeSpan.FromMinutes(rateLimiterConf.PressureTimeSpan);
        _requestCount = rateLimiterConf.Count;
        _saveEveryNthIncrement = rateLimiterConf.SaveEveryNthIncrement;

        _botContext = botContext;
    }

    public async Task<int> CountAsync(long chatId)
    {
        var history = await GetHistoryAsync(chatId, DateTime.UtcNow);

        return history.Values.Count;
    }

    private async Task<ReverseSearchRequestLimiterHistory> GetHistoryAsync(long chatId, DateTime now)
    {
        using var _ = _mutex.Lock();
        if (!_chats.TryGetValue(chatId, out var history))
        {
            var mongoHistory = await _botContext.ReverseSearchRequestHistory.FirstOrDefaultAsync(x => x.Id == chatId);
            history = new ReverseSearchRequestLimiterHistory
            {
                Lock = new AsyncLock(),
                Values = new Queue<DateTime>(mongoHistory == null ? [] : mongoHistory.Values),
                Increment = mongoHistory?.Increment ?? 0
            };
            _chats[chatId] = history;
        }

        using var chatLock = history.Lock.Lock();
        while (history.Values.Count > 0)
        {
            var earliest = history.Values.Peek();
            var elapsed = now.Subtract(earliest);
            if (elapsed < _expirationTime)
                break;

            history.Values.Dequeue();
        }

        return history;
    }

    public async Task<(bool, int)> NextAsync(long chatId)
    {
        var now = DateTime.UtcNow;

        var history = await GetHistoryAsync(chatId, now);
        using var chatLock = await history.Lock.LockAsync();

        if (history.Values.Count >= _requestCount)
            return (false, 0);

        var pressure = history.Values.Count(x => now.Subtract(x) < _pressureTimeSpan);
        history.Values.Enqueue(now);
        history.Increment++;

        if (history.Values.Count == _requestCount)
        {
            // todo: sent a message to the chat owner when the rate limit is hit
        }

        if (history.Increment % _saveEveryNthIncrement == 0)
        {
            await _botContext.ReverseSearchRequestHistory.UpsertAsync(x => x.Id == chatId, new MongoReverseSearchRequestHistory
            {
                Id = chatId,
                Values = history.Values.ToArray(),
                Increment = history.Increment
            });
        }

        return (true, pressure);
    }
}
