using AsyncKeyedLock;
using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor;

public class ProcessWorker : BackgroundService
{
    private static readonly AsyncKeyedLocker<string> Mutex = new();

    private readonly IBlueskyEventRepository _eventRepository;
    private readonly BlueskyEventHandlerFactory _eventHandlerFactory;
    private readonly Channel<BlueskyEvent> _channel;
    private readonly IOptions<BlueskyJetstreamProcessorOptions> _options;
    private readonly ILogger<ProcessWorker> _logger;
    private readonly TelemetryClient _telemetryClient;

    public ProcessWorker(
        IBlueskyEventRepository eventRepository,
        BlueskyEventHandlerFactory eventHandlerFactory,
        Channel<BlueskyEvent> channel,
        IOptions<BlueskyJetstreamProcessorOptions> options,
        ILogger<ProcessWorker> logger,
        TelemetryClient telemetryClient)
    {
        _eventRepository = eventRepository;
        _eventHandlerFactory = eventHandlerFactory;
        _channel = channel;
        _options = options;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var blueskyEvent = await _channel.Reader.ReadAsync(stoppingToken);
            var eventHandler = blueskyEvent.Visit(_eventHandlerFactory);

            _telemetryClient.GetMetric(blueskyEvent.GetType().Name).TrackValue(1);

            using (await Mutex.LockAsync(blueskyEvent.Did, stoppingToken))
            {
                try
                {
                    await eventHandler.RunAsync();
                    await _eventRepository.DeleteAsync(blueskyEvent.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception occurred handling an event. Waiting for {Interval} before processing the next one.", _options.Value.ErrorDelay);
                    await Task.Delay(_options.Value.ErrorDelay!.Value, stoppingToken);
                }
            }
        }
    }
}
