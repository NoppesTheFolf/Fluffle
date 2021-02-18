using Flurl.Http;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http
{
    /// <summary>
    /// Provides ways to retry failing HTTP requests made with the Flurl library.
    /// </summary>
    public static class HttpResiliency
    {
        /// <summary>
        /// Run the function provided as <paramref name="request"/> indefinitely as long as the
        /// exceptions thrown by said function are transient (and therefore worth retrying).
        /// </summary>
        public static async Task RunAsync(Func<Task> request, Action onTimeout = null,
            Action<FlurlHttpException> onHttpException = null, Action<TimeSpan> onRetry = null)
        {
            await HttpRetryPolicy
                .BuildFlurlRetryPolicy<bool>(onTimeout, onHttpException, onRetry)
                .Invoke(async () =>
                {
                    await request();

                    return true;
                });
        }

        /// <summary>
        /// Run the function provided as <paramref name="request"/> indefinitely as long as the
        /// exceptions thrown by said function are transient (and therefore worth retrying).
        /// </summary>
        public static async Task<T> RunAsync<T>(Func<Task<T>> request, Action onTimeout = null,
            Action<FlurlHttpException> onHttpException = null, Action<TimeSpan> onRetry = null)
        {
            return await HttpRetryPolicy
                .BuildFlurlRetryPolicy<T>(onTimeout, onHttpException, onRetry)
                .Invoke(request);
        }
    }
}
