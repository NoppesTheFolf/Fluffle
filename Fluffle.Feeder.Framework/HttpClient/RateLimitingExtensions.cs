using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Fluffle.Feeder.Framework.HttpClient;

public static class RateLimitingExtensions
{
    public static IHttpClientBuilder AddPacedRateLimit(this IHttpClientBuilder builder, Func<IServiceProvider, TimeSpan> getDelay)
    {
        return builder.AddHttpMessageHandler(provider => new RateLimitingHandler(new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                QueueLimit = int.MaxValue,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                ReplenishmentPeriod = getDelay(provider),
                TokenLimit = 1,
                TokensPerPeriod = 1
            })));
    }
}
