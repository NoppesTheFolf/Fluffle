using System.Collections.Generic;

namespace Fluffle.TelegramBot;

public class BotConfiguration
{
    public string TelegramToken { get; set; }

    public string TelegramHost { get; set; }

    public int TelegramGlobalBurstLimit { get; set; }

    public int TelegramGlobalBurstInterval { get; set; }

    public int TelegramGroupBurstLimit { get; set; }

    public int TelegramGroupBurstInterval { get; set; }

    public ICollection<string> TelegramKnownSources { get; set; }

    public string MongoConnectionString { get; set; }

    public string MongoDatabase { get; set; }

    public class ReverseSearchConfiguration
    {
        public int Workers { get; set; }

        public class RateLimiterConfiguration
        {
            public int Count { get; set; }

            public int ExpirationTime { get; set; }

            public int PressureTimeSpan { get; set; }

            public int SaveEveryNthIncrement { get; set; }
        }

        public RateLimiterConfiguration RateLimiter { get; set; }
    }

    public ReverseSearchConfiguration ReverseSearch { get; set; }

    public class CleanerConfiguration
    {
        public int Interval { get; set; }

        public int ExpirationTime { get; set; }
    }

    public CleanerConfiguration MessageCleaner { get; set; }
}
