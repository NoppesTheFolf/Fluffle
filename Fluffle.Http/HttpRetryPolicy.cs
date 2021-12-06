using Flurl.Http;
using Polly;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http
{
    /// <summary>
    /// Contains the policy used for handle failing HTTP requests.
    /// </summary>
    public static class HttpRetryPolicy
    {
        /// <summary>
        /// The amount of time between each HTTP request retry while the amount of retries that have
        /// occured is less than or equal to <see cref="MaximumShortHttpIntervalRetryCount"/>.
        /// </summary>
        private static readonly TimeSpan ShortHttpRetryInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The amount of time between each HTTP request retry after the amount of retries that have
        /// occured is more than <see cref="MaximumShortHttpIntervalRetryCount"/>.
        /// </summary>
        private static readonly TimeSpan LongHttpRetryInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The maximum amount of HTTP request retries that might occur while using <see cref="ShortHttpRetryInterval"/>.
        /// </summary>
        private const int MaximumShortHttpIntervalRetryCount = 5;

        public static Func<Func<Task<TResult>>, Task<TResult>> BuildFlurlRetryPolicy<TResult>(Action onTimeout = null, Action<FlurlHttpException> onHttpException = null, Action<TimeSpan> onRetry = null, params int[] statusCodes)
        {
            return Policy<TResult>
                .Handle<FlurlHttpTimeoutException>(exception =>
                {
                    onTimeout?.Invoke();

                    return true;
                })
                .Or<FlurlHttpException>(exception =>
                {
                    // Handle any network related exceptions
                    if (exception.InnerException is HttpRequestException)
                    {
                        onHttpException?.Invoke(exception);
                        return true;
                    }

                    return exception.IsTransient(statusCodes);
                })
                .WaitAndRetryForeverAsync(retryAttempt =>
                {
                    var timeout = retryAttempt switch
                    {
                        _ when retryAttempt >= 1 && retryAttempt <= MaximumShortHttpIntervalRetryCount => ShortHttpRetryInterval,
                        _ => LongHttpRetryInterval
                    };

                    onRetry?.Invoke(timeout);

                    return timeout;
                }).ExecuteAsync;
        }

        /// <summary>
        /// Whether the failed request should be considered transient or not.
        /// </summary>
        public static bool IsTransient(this FlurlHttpException exception, params int[] statusCodes)
        {
            // Handle any network related exceptions
            if (exception.InnerException is HttpRequestException)
                return true;

            var httpStatusCode = exception.Call.Response.StatusCode;

            // 408: The server decided the request took too long to send, who knows, might
            // be the servers fault

            // 522: Cloudflare returns a 522 Origin Connection Timeout if the requests from
            // Cloudflare its edge servers timeout.

            // 502: If the origin server is not properly configured, Cloudflare returns a
            // 502. However, this might simply be because of a restart, so worth retrying.

            // 503: For whatever reason the server is unable to process requests at this moment.
            // However, it is safe to assume this is a temporary failure.

            // 520: If the origin server did something unexpected with the response sent
            // Cloudflare its servers, Cloudflare returns a 520, however, this generally
            // solves itself.

            // 521: Sometimes Cloudflare isn't able to connect to the origin server because the
            // server has lost connection to the internet. This should solve itself
            // eventually, most of the time...

            return statusCodes.Contains(httpStatusCode) || httpStatusCode is 408 or 502 or 503 or 504 or 520 or 521 or 522 or 523 or 524;
        }
    }
}
