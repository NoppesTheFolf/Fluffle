using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluffle.TelegramBot.Database.Entities;
using Nito.AsyncEx;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Fluffle.TelegramBot.Routing;

public sealed class BurstRateLimiterScope : IDisposable
{
    private readonly Queue<DateTime> _history;
    private readonly IDisposable _lock;

    public BurstRateLimiterScope(Queue<DateTime> history, IDisposable @lock)
    {
        _history = history;
        _lock = @lock;
    }

    public void Dispose()
    {
        _history.Enqueue(DateTime.UtcNow);
        _lock?.Dispose();
    }
}

public class BurstRateLimiter
{
    private readonly AsyncLock _mutex;
    private readonly Queue<DateTime> _history;
    private readonly int _burstLimit;
    private readonly TimeSpan _burstInterval;

    public BurstRateLimiter(int burstLimit, int burstInterval)
    {
        _mutex = new AsyncLock();
        _history = new Queue<DateTime>();
        _burstLimit = burstLimit;
        _burstInterval = TimeSpan.FromMilliseconds(burstInterval);
    }

    public async Task<BurstRateLimiterScope> NextAsync()
    {
        var @lock = await _mutex.LockAsync();

        // Clear the history of any old request times
        var now = DateTime.UtcNow;
        while (_history.Count > 0)
        {
            var earliestRequest = _history.Peek();
            if (now.Subtract(earliestRequest) < _burstInterval)
                break;

            _history.Dequeue();
        }

        // Check if the burst limit has been reached
        if (_history.Count >= _burstLimit)
        {
            var earliestRequest = _history.Peek();
            var timeToWait = earliestRequest.Add(_burstInterval).Subtract(now);

            if (timeToWait > TimeSpan.Zero)
                await Task.Delay(timeToWait);
        }

        return new BurstRateLimiterScope(_history, @lock);
    }
}

public static class RateLimiter
{
    public const int GlobalBurstLimit = 29;

    public const int GlobalBurstInterval = 1000;

    public const int GroupBurstLimit = 19;

    public const int GroupBurstInterval = 60000;


    private static readonly BurstRateLimiter GlobalRateLimiter = new(GlobalBurstLimit, GlobalBurstInterval);

    private static readonly ConcurrentDictionary<long, BurstRateLimiter> GroupRateLimiters = new();

    public static async Task RunAsync(MongoChat mongoChat, Func<Task> makeRequest) => await RunAsync(mongoChat.Id, mongoChat.Type, makeRequest);

    public static async Task RunAsync(Chat chat, Func<Task> makeRequest) => await RunAsync(chat.Id, chat.Type, makeRequest);

    public static async Task RunAsync(long chatId, ChatType chatType, Func<Task> makeRequest)
    {
        await RunAsync(chatId, chatType, async () =>
        {
            await makeRequest();

            return false;
        });
    }

    public static async Task<T> RunAsync<T>(MongoChat mongoChat, Func<Task<T>> makeRequest) => await RunAsync(mongoChat.Id, mongoChat.Type, makeRequest);

    public static async Task<T> RunAsync<T>(Chat chat, Func<Task<T>> makeRequest) => await RunAsync(chat.Id, chat.Type, makeRequest);

    public static async Task<T> RunAsync<T>(long chatId, ChatType chatType, Func<Task<T>> makeRequest)
    {
        BurstRateLimiterScope groupRateLimiterScope = null;
        try
        {
            if (chatType is ChatType.Group or ChatType.Supergroup or ChatType.Channel)
            {
                var rateLimiter = GroupRateLimiters.GetOrAdd(chatId, _ => new BurstRateLimiter(GroupBurstLimit, GroupBurstInterval));
                groupRateLimiterScope = await rateLimiter.NextAsync();
            }

            using var _ = await GlobalRateLimiter.NextAsync();
            return await makeRequest();
        }
        finally
        {
            groupRateLimiterScope?.Dispose();
        }
    }
}
