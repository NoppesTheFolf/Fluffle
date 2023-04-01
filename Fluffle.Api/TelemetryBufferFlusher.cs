using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Telemetry;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api;

public class TelemetryBufferFlusher : IHostedService
{
    private readonly ITelemetryClientFactory _telemetryClientFactory;
    private readonly ILogger<TelemetryBufferFlusher> _logger;

    public TelemetryBufferFlusher(ITelemetryClientFactory telemetryClientFactory, ILogger<TelemetryBufferFlusher> logger)
    {
        _telemetryClientFactory = telemetryClientFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start flushing telemetry buffer.");
        var client = _telemetryClientFactory.Create(string.Empty);
        await client.FlushAsync();
        _logger.LogInformation("Finished flushing telemetry buffer.");
    }
}
