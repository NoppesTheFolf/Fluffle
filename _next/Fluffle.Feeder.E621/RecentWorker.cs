using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Ingestion.Api.Client;
using Microsoft.Extensions.Options;
using Noppes.E621;

namespace Fluffle.Feeder.E621;

public class RecentWorker : BackgroundService
{
    private readonly IE621Client _e621Client;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<E621FeederState> _stateRepository;
    private readonly IOptions<E621FeederOptions> _options;
    private readonly ILogger<RecentWorker> _logger;

    public RecentWorker(
        IE621Client e621Client,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        IOptions<E621FeederOptions> options,
        ILogger<RecentWorker> logger)
    {
        _e621Client = e621Client;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<E621FeederState>("E621Recent");
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var state = await _stateRepository.GetAsync() ?? new E621FeederState
        {
            LastRunWhen = DateTime.MinValue,
            CurrentId = null
        };

        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var timeToWait = _options.Value.RecentRunInterval - (DateTime.UtcNow - state.LastRunWhen);
            if (timeToWait > TimeSpan.Zero)
            {
                _logger.LogInformation("Waiting for {Interval} before running again.", timeToWait);
                await Task.Delay(timeToWait, stoppingToken);
            }

            var feeder = new E621Feeder(_e621Client, _ingestionApiClient, _stateRepository, _logger);
            await feeder.RunAsync(state, DateTime.UtcNow.Subtract(_options.Value.RecentRetrievePeriod), stoppingToken);

            state.CurrentId = null;
            state.LastRunWhen = DateTime.UtcNow;
            await _stateRepository.PutAsync(state);
        }
    }
}
