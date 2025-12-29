using System.Threading.RateLimiting;

namespace Fluffle.Feeder.Framework.HttpClient;

internal class RateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _rateLimiter;

    public RateLimitingHandler(RateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired)
        {
            throw new HttpRequestException("Rate limit exceeded.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
