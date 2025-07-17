using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.FurAffinity.Client;
using Fluffle.Ingestion.Api.Client;
using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.FurAffinity.Workers;

internal class AgedWorker : BackgroundService
{
    private readonly FurAffinityClient _furAffinityClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<FurAffinityFeederState> _stateRepository;
    private readonly ILogger<NewestWorker> _logger;
    private readonly IOptions<FurAffinityFeederOptions> _options;

    public AgedWorker(
        FurAffinityClient furAffinityClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        ILogger<NewestWorker> logger,
        IOptions<FurAffinityFeederOptions> options)
    {
        _furAffinityClient = furAffinityClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<FurAffinityFeederState>("FurAffinityAged");
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _stateRepository.GetAsync();

            if (state == null)
            {
                var newestId = await _furAffinityClient.GetNewestIdAsync();
                state = new FurAffinityFeederState
                {
                    CurrentId = newestId,
                    EndId = -1,
                    StartId = -1
                };
                await _stateRepository.PutAsync(state);
            }

            var feeder = new FurAffinityFeeder(_furAffinityClient, _ingestionApiClient, _stateRepository, _logger);
            await feeder.RunUntilAsync(state, _options.Value.MaximumAge, stoppingToken);

            _logger.LogInformation("All submissions have been retrieved from {Ago} ago. Waiting for {Interval} before running again.", _options.Value.MaximumAge, _options.Value.AgedRunInterval);
            await Task.Delay(_options.Value.AgedRunInterval, stoppingToken);
        }
        stoppingToken.ThrowIfCancellationRequested();
    }
}
