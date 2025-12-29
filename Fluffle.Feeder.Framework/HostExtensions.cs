using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fluffle.Feeder.Framework;

// Original code and idea: https://github.com/dotnet/runtime/issues/67146#issuecomment-2377856058
public static class HostExtensions
{
    public static async Task RunAndSetExitCodeAsync(this IHost host)
    {
        var backgroundServices = host.Services
            .GetServices<IHostedService>()
            .OfType<BackgroundService>()
            .ToList();

        await host.RunAsync();

        if (backgroundServices.Any(x => x.ExecuteTask?.IsFaulted == true))
        {
            Environment.ExitCode = 1;
        }
    }
}
