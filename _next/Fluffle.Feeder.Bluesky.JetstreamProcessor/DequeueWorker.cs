using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using System.Threading.Channels;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor;

public class DequeueWorker : BackgroundService
{
    private readonly IBlueskyEventRepository _eventRepository;
    private readonly Channel<BlueskyEvent> _channel;
    private readonly ILogger<DequeueWorker> _logger;

    public DequeueWorker(
        IBlueskyEventRepository eventRepository,
        Channel<BlueskyEvent> channel,
        ILogger<DequeueWorker> logger)
    {
        _eventRepository = eventRepository;
        _channel = channel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _channel.Writer.WaitToWriteAsync(stoppingToken);

            var blueskyEvent = await _eventRepository.GetHighestPriorityAsync();
            if (blueskyEvent == null)
            {
                var delay = TimeSpan.FromSeconds(1);
                await Task.Delay(delay, stoppingToken);
                continue;
            }

            if (blueskyEvent.AttemptCount >= 10)
            {
                _logger.LogInformation("Giving up on processing post from {Did} with ID {RKey} after 10 tries.", blueskyEvent.Did, blueskyEvent.Id);
                await _eventRepository.DeleteAsync(blueskyEvent.Id);
                continue;
            }

            var visibleWhen = DateTime.UtcNow.Add(GetProcessingTimeout(blueskyEvent.AttemptCount));
            await _eventRepository.IncrementAttemptCountAsync(blueskyEvent.Id, visibleWhen);

            await _channel.Writer.WriteAsync(blueskyEvent, stoppingToken);
        }
    }

    private static TimeSpan GetProcessingTimeout(int attemptCount)
    {
        // 15m, 30m, 1h, 2h, 4h, 1d, 1d, ...

        // Limit the timeout to a day after having already tried 5 times
        if (attemptCount >= 5)
            return TimeSpan.FromDays(1);

        return TimeSpan.FromMinutes(15 * Math.Pow(2, attemptCount));
    }
}
