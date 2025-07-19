namespace Fluffle.Ingestion.Worker;

internal static class HostExtensions
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
