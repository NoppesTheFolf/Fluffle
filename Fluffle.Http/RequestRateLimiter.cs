using Flurl.Http;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http
{
    /// <summary>
    /// Request interceptor used to limit the rate of requests.
    /// </summary>
    public class RequestRateLimiter : ICallInterceptor
    {
        private readonly AsyncLock _mutex;
        private readonly long _intervalInMilliseconds;
        private long _waitUntil;

        public RequestRateLimiter(TimeSpan interval)
        {
            _intervalInMilliseconds = (int)Math.Round(interval.TotalMilliseconds, MidpointRounding.AwayFromZero);
            _waitUntil = -1;

            _mutex = new AsyncLock();
        }

        public async Task InterceptAsync(FlurlCall call)
        {
            using var _ = await _mutex.LockAsync();

            var ttw = _waitUntil == -1 ? 0 : (int)(_waitUntil - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            if (ttw > 0)
                await Task.Delay(ttw);

            _waitUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _intervalInMilliseconds;
        }
    }
}
