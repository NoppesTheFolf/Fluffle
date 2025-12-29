using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Weasyl.ApiClient;
using Fluffle.Ingestion.Api.Client;

namespace Fluffle.Feeder.Weasyl.Workers;

internal class ArchiveWorker : BackgroundService
{
    private readonly WeasylApiClient _weasylApiClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<WeasylFeederState> _stateRepository;
    private readonly ILogger<ArchiveWorker> _logger;

    public ArchiveWorker(
        WeasylApiClient weasylApiClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        ILogger<ArchiveWorker> logger)
    {
        _weasylApiClient = weasylApiClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<WeasylFeederState>("WeasylArchive");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _stateRepository.GetAsync();
            if (state == null)
            {
                var newestId = await _weasylApiClient.GetNewestIdAsync();
                state = new WeasylFeederState
                {
                    StartSubmissionId = newestId,
                    EndSubmissionId = 0,
                    CurrentSubmissionId = newestId
                };
            }

            var feeder = new WeasylFeeder(_weasylApiClient, _ingestionApiClient, _stateRepository, _logger);
            await feeder.RunAsync(state, stoppingToken);

            // We've processed the site from beginning to end, now we start over
            state.StartSubmissionId = await _weasylApiClient.GetNewestIdAsync();
            state.EndSubmissionId = 0;
            state.CurrentSubmissionId = state.StartSubmissionId;

            await _stateRepository.PutAsync(state);
        }
        stoppingToken.ThrowIfCancellationRequested();
    }
}
