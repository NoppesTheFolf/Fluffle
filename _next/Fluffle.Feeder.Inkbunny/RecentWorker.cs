using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Inkbunny.Client;
using Fluffle.Ingestion.Api.Client;
using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.Inkbunny;

internal class RecentWorker : BackgroundService
{
    private readonly InkbunnyClient _inkbunnyClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<InkbunnyFeederState> _stateRepository;
    private readonly IOptions<InkbunnyFeederOptions> _options;
    private readonly ILogger<RecentWorker> _logger;

    public RecentWorker(
        InkbunnyClient inkbunnyClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        IOptions<InkbunnyFeederOptions> options,
        ILogger<RecentWorker> logger)
    {
        _inkbunnyClient = inkbunnyClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<InkbunnyFeederState>("InkbunnyRecent");
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var state = await _stateRepository.GetAsync() ?? new InkbunnyFeederState
        {
            LastRunWhen = DateTime.MinValue,
            CurrentId = 0,
            RetrieveUntil = DateTime.UtcNow.Subtract(_options.Value.RecentRetrievePeriod)
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var timeToWait = _options.Value.RecentRunInterval - (DateTime.UtcNow - state.LastRunWhen);
            if (timeToWait > TimeSpan.Zero)
            {
                _logger.LogInformation("Waiting for {Interval} before running again.", timeToWait);
                await Task.Delay(timeToWait, stoppingToken);
            }

            var response = await _inkbunnyClient.SearchSubmissionsAsync();
            state.CurrentId = int.Parse(response.Submissions.MaxBy(x => x.CreatedWhen)!.Id);

            var feeder = new InkbunnyFeeder(_inkbunnyClient, _ingestionApiClient, _stateRepository, _logger);
            await feeder.RunAsync(state, stoppingToken);

            state.LastRunWhen = DateTime.UtcNow;
            state.RetrieveUntil = state.LastRunWhen.Subtract(_options.Value.RecentRetrievePeriod);
            await _stateRepository.PutAsync(state);
        }
        stoppingToken.ThrowIfCancellationRequested();
    }
}
