using Flurl.Http;
using Noppes.Fluffle.Http;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.E621Sync;

/// <summary>
/// Provides ways to retry failing HTTP requests made to e621 with the Flurl library.
/// </summary>
public static class E621HttpResiliency
{
    /// <summary>
    /// e621, for whatever reason, randomly throws 501 Not Implemented errors sometimes.
    /// </summary>
    private static readonly int[] E621StatusCodes = { 501 };

    /// <summary>
    /// Run the function provided as <paramref name="request"/> indefinitely as long as the
    /// exceptions thrown by said function are transient (and therefore worth retrying).
    /// </summary>
    public static Task RunAsync(Func<Task> request, Action onTimeout = null,
        Action<FlurlHttpException> onHttpException = null, Action<TimeSpan> onRetry = null)
    {
        return HttpResiliency.RunAsync(request, onTimeout, onHttpException, onRetry, E621StatusCodes);
    }

    /// <summary>
    /// Run the function provided as <paramref name="request"/> indefinitely as long as the
    /// exceptions thrown by said function are transient (and therefore worth retrying).
    /// </summary>
    public static Task<T> RunAsync<T>(Func<Task<T>> request, Action onTimeout = null,
        Action<FlurlHttpException> onHttpException = null, Action<TimeSpan> onRetry = null, params int[] statusCodes)
    {
        return HttpResiliency.RunAsync(request, onTimeout, onHttpException, onRetry, E621StatusCodes);
    }
}
