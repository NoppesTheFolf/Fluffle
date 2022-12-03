using Flurl.Http;
using Humanizer;
using Nito.AsyncEx;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot
{
    public record ReverseSearchRequestLimiterHistory
    {
        public AsyncLock Lock { get; set; }

        public Queue<DateTime> Values { get; set; }

        public int Increment { get; set; }
    }

    public class ReverseSearchRequestLimiter
    {
        private readonly AsyncLock _mutex = new();
        private readonly Dictionary<long, ReverseSearchRequestLimiterHistory> _chats = new();

        private readonly TimeSpan _expirationTime;
        private readonly TimeSpan _pressureTimeSpan;
        private readonly int _requestCount;
        private readonly int _saveEveryNthIncrement;

        private readonly BotContext _botContext;

        public ReverseSearchRequestLimiter(BotConfiguration configuration, BotContext botContext)
        {
            var rateLimiterConf = configuration.ReverseSearch.RateLimiter;

            _expirationTime = rateLimiterConf.ExpirationTime.Minutes();
            _pressureTimeSpan = rateLimiterConf.PressureTimeSpan.Minutes();
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
                    Values = new Queue<DateTime>(mongoHistory == null ? Array.Empty<DateTime>() : mongoHistory.Values),
                    Increment = mongoHistory?.Increment ?? default
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
                return (false, default);

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

    public class ReverseSearchSchedulerItem : WorkSchedulerItem<FluffleResponse>
    {
        public Stream Stream { get; set; }

        public bool IncludeNsfw { get; set; }

        public int Limit { get; set; }
    }

    public class ReverseSearchScheduler : WorkScheduler<ReverseSearchSchedulerItem, int, FluffleResponse>
    {
        private readonly FluffleClient _fluffleClient;

        public ReverseSearchScheduler(int numberOfWorkers, FluffleClient fluffleClient) : base(numberOfWorkers)
        {
            _fluffleClient = fluffleClient;
        }

        protected override async Task<FluffleResponse> HandleAsync(ReverseSearchSchedulerItem item)
        {
            return await _fluffleClient.SearchAsync(item.Stream, item.IncludeNsfw, item.Limit);
        }
    }

    public class FluffleClient : ApiClient
    {
        public FluffleClient(string applicationName, string baseUrl = "https://api.fluffle.xyz") : base(baseUrl)
        {
            FlurlClient.Headers.Add("User-Agent", Project.UserAgent(applicationName));
        }

        public async Task<FluffleResponse> SearchAsync(Stream stream, bool includeNsfw, int limit)
        {
            var platforms = Enum.GetNames<FlufflePlatform>();
            var response = await Request("v1", "search")
                .PostMultipartAsync(content =>
                {
                    content.AddFile("file", stream, "dummy");
                    content.AddString("includeNsfw", includeNsfw.ToString());
                    content.AddString("limit", limit.ToString());

                    foreach (var platform in platforms)
                        content.AddString("platforms", platform);
                });

            return await response.GetJsonAsync<FluffleResponse>();
        }
    }

    public class FluffleStats
    {
        public int ElapsedMilliseconds { get; set; }

        public int Count { get; set; }
    }

    public enum FluffleMatch
    {
        Exact = 1,
        TossUp = 2,
        Alternative = 3,
        Unlikely = 4
    }

    public class FluffleResult
    {
        public int Id { get; set; }

        public float Score { get; set; }

        public FluffleMatch Match { get; set; }

        public FlufflePlatform Platform { get; set; }

        public string Location { get; set; }

        public bool IsSfw { get; set; }

        public class FluffleThumbnail
        {
            public int Width { get; set; }

            public int CenterX { get; set; }

            public int Height { get; set; }

            public int CenterY { get; set; }

            public string Location { get; set; }
        }

        public FluffleThumbnail Thumbnail { get; set; }

        public class FluffleCredit
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public ICollection<FluffleCredit> Credits { get; set; }
    }

    public class FluffleResponse
    {
        public FluffleStats Stats { get; set; }

        public IList<FluffleResult> Results { get; set; }
    }

    public enum FlufflePlatform
    {
        E621 = 1,
        FurryNetwork = 2,
        FurAffinity = 3,
        Weasyl = 4,
        Twitter = 5
    }

    public static class FlufflePlatformExtensions
    {
        public static string Pretty(this FlufflePlatform platform)
        {
            return platform switch
            {
                FlufflePlatform.E621 => "e621",
                FlufflePlatform.FurryNetwork => "Furry Network",
                FlufflePlatform.FurAffinity => "Fur Affinity",
                FlufflePlatform.Weasyl => "Weasyl",
                FlufflePlatform.Twitter => "Twitter",
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }

        public static int Priority(this FlufflePlatform platform)
        {
            return platform switch
            {
                FlufflePlatform.E621 => 3,
                FlufflePlatform.FurryNetwork => 5,
                FlufflePlatform.FurAffinity => 1,
                FlufflePlatform.Weasyl => 4,
                FlufflePlatform.Twitter => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
    }
}
