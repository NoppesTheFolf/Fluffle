using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices;

/// <summary>
/// Signals all of the <see cref="ServiceBuilder"/> instances to shutdown gracefully on
/// application shutdown request.
/// </summary>
public class ServiceShutdownSignaler : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => ServiceBuilder.Shutdown();
}
