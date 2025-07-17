using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.FurAffinity.Client;
using Fluffle.Ingestion.Api.Client;
using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.FurAffinity.Workers;

internal class NewestWorker : BackgroundService
{
    private readonly FurAffinityClient _furAffinityClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<FurAffinityFeederState> _stateRepository;
    private readonly ILogger<NewestWorker> _logger;
    private readonly IOptions<FurAffinityFeederOptions> _options;

    public NewestWorker(
        FurAffinityClient furAffinityClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        ILogger<NewestWorker> logger,
        IOptions<FurAffinityFeederOptions> options)
    {
        _furAffinityClient = furAffinityClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<FurAffinityFeederState>("FurAffinityNewest");
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _stateRepository.GetAsync();

            if (state != null)
            {
                await RunFeederAsync(state, stoppingToken);
            }

            var newestId = await _furAffinityClient.GetNewestIdAsync();
            state ??= new FurAffinityFeederState
            {
                StartId = newestId,
                CurrentId = newestId,
                EndId = newestId
            };
            state.StartId = newestId;
            state.CurrentId = newestId;

            await RunFeederAsync(state, stoppingToken);

            _logger.LogInformation("All new submissions have been retrieved. Waiting for {Interval} before running again.", _options.Value.NewestRunInterval);
            await Task.Delay(_options.Value.NewestRunInterval, stoppingToken);
        }
        stoppingToken.ThrowIfCancellationRequested();
    }

    private async Task RunFeederAsync(FurAffinityFeederState state, CancellationToken cancellationToken)
    {
        var feeder = new FurAffinityFeeder(_furAffinityClient, _ingestionApiClient, _stateRepository, _logger);
        await feeder.RunDecrementAsync(state, cancellationToken);

        state.EndId = state.StartId;
        await _stateRepository.PutAsync(state);
    }
}
