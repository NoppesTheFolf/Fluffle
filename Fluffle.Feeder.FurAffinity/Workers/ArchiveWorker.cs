using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.FurAffinity.Client;
using Fluffle.Ingestion.Api.Client;

namespace Fluffle.Feeder.FurAffinity.Workers;

internal class ArchiveWorker : BackgroundService
{
    private readonly FurAffinityClient _furAffinityClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<FurAffinityFeederState> _stateRepository;
    private readonly ILogger<ArchiveWorker> _logger;

    public ArchiveWorker(
        FurAffinityClient furAffinityClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        ILogger<ArchiveWorker> logger)
    {
        _furAffinityClient = furAffinityClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<FurAffinityFeederState>("FurAffinityArchive");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _stateRepository.GetAsync();

            if (state == null || state.CurrentId == 0)
            {
                var newestId = await _furAffinityClient.GetNewestIdAsync();
                state = new FurAffinityFeederState
                {
                    StartId = newestId,
                    EndId = 0,
                    CurrentId = newestId
                };
            }

            var feeder = new FurAffinityFeeder(_furAffinityClient, _ingestionApiClient, _stateRepository, _logger);
            await feeder.RunDecrementAsync(state, stoppingToken);
        }
        stoppingToken.ThrowIfCancellationRequested();
    }
}
