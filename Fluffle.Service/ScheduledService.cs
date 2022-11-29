using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service;

public abstract class ScheduledService<TService> : Service<TService> where TService : Service
{
    protected abstract TimeSpan Interval { get; }

    private readonly ILogger<TService> _logger;

    protected ScheduledService(IServiceProvider services) : base(services)
    {
        _logger = services.GetRequiredService<ILogger<TService>>();
    }

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            var start = DateTime.UtcNow;
            await RunAsync(stoppingToken);
            var end = DateTime.UtcNow;

            var elapsed = end - start;
            var timeToWait = Interval - elapsed;
            if (timeToWait > TimeSpan.Zero)
            {
                _logger.LogInformation("Waiting for {time} before running again.", timeToWait);
                await Task.Delay(timeToWait, stoppingToken);
            }
            else
            {
                _logger.LogInformation("More time elapsed than the interval. Running again immediately.");
            }
        }
    }

    protected abstract Task RunAsync(CancellationToken stoppingToken);
}