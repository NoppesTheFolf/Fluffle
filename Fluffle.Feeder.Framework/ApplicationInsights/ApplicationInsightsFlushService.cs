using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fluffle.Feeder.Framework.ApplicationInsights;

internal class ApplicationInsightsFlushService : IHostedService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ApplicationInsightsFlushService> _logger;

    public ApplicationInsightsFlushService(TelemetryClient telemetryClient, ILogger<ApplicationInsightsFlushService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start flushing Application Insights telemetry.");
        await _telemetryClient.FlushAsync(cancellationToken);
        _logger.LogInformation("Application Insights telemetry flushed.");
    }
}
