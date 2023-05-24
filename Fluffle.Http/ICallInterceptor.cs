using Flurl.Http;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http;

/// <summary>
/// Intercepts calls made by by <see cref="ApiClient"/> implementations.
/// </summary>
public interface ICallInterceptor
{
    public Task InterceptBeforeAsync(FlurlCall call);

    public Task InterceptAfterAsync(FlurlCall call);
}
