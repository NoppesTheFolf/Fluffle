using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Weasyl.ApiClient;
using Fluffle.Ingestion.Api.Client;
using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.Weasyl.Workers;

internal class NewestWorker : BackgroundService
{
    private readonly WeasylApiClient _weasylApiClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<WeasylFeederState> _stateRepository;
    private readonly IOptions<WeasylFeederOptions> _options;
    private readonly ILogger<NewestWorker> _logger;

    public NewestWorker(
        WeasylApiClient weasylApiClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        IOptions<WeasylFeederOptions> options,
        ILogger<NewestWorker> logger)
    {
        _weasylApiClient = weasylApiClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<WeasylFeederState>("WeasylNewest");
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _stateRepository.GetAsync();
            var feeder = new WeasylFeeder(_weasylApiClient, _ingestionApiClient, _stateRepository, _logger);

            // If there is still work left to do from the previous run. We need to finish that first
            if (state != null)
            {
                await feeder.RunAsync(state, stoppingToken);
            }

            var newestId = await _weasylApiClient.GetNewestIdAsync();
            state ??= new WeasylFeederState
            {
                StartSubmissionId = newestId,
                CurrentSubmissionId = newestId,
                EndSubmissionId = newestId
            };
            state.StartSubmissionId = newestId;
            state.CurrentSubmissionId = newestId;

            await feeder.RunAsync(state, stoppingToken);

            _logger.LogInformation("All new submissions have been retrieved. Waiting for {Interval} before running again.", _options.Value.NewestRunInterval);
            await Task.Delay(_options.Value.NewestRunInterval, stoppingToken);
        }
        stoppingToken.ThrowIfCancellationRequested();
    }
}
