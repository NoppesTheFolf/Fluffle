using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Legacy.MainApi;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.Legacy;

public class Worker : BackgroundService
{
    private readonly MainApiClient _mainApiClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<LegacyFeederState> _stateRepository;
    private readonly IOptions<LegacyFeederOptions> _options;
    private readonly ILogger<Worker> _logger;

    public Worker(
        MainApiClient mainApiClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepositoryFactory stateRepositoryFactory,
        IOptions<LegacyFeederOptions> options,
        ILogger<Worker> logger)
    {
        _mainApiClient = mainApiClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepositoryFactory.Create<LegacyFeederState>("Legacy");
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _stateRepository.GetAsync() ?? new LegacyFeederState
            {
                LastRunWhen = null,
                Platforms = new Dictionary<string, long>()
            };

            if (state.LastRunWhen != null)
            {
                var timeToWait = _options.Value.RunInterval - (DateTime.UtcNow - state.LastRunWhen.Value);
                if (timeToWait > TimeSpan.Zero)
                {
                    _logger.LogInformation("Waiting for {Interval} before running again.", timeToWait);
                    await Task.Delay(timeToWait, stoppingToken);
                }
            }

            var lastRunWhen = DateTime.UtcNow;
            foreach (var platform in _options.Value.Platforms)
            {
                await ProcessAsync(state, platform, stoppingToken);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            state.LastRunWhen = lastRunWhen;
            await _stateRepository.PutAsync(state);
        }
    }

    private async Task ProcessAsync(LegacyFeederState state, LegacyFeederOptions.Platform platform, CancellationToken cancellationToken)
    {
        using var _ = _logger.BeginScope("Platform:{Platform}", platform.Id);

        var changeId = state.Platforms.TryGetValue(platform.Id, out var changeIdValue) ? changeIdValue : 0;

        var creditableEntitiesLookup = new Dictionary<int, CreditableEntityModel>();
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Getting images after change ID {ChangeId}.", changeId);

            var images = await _mainApiClient.GetImagesAsync(platform.Id, changeId);
            if (images.Results.Count == 0)
            {
                break;
            }

            if (images.Results.Where(x => !x.IsDeleted).SelectMany(x => x.Credits).Any(x => !creditableEntitiesLookup.ContainsKey(x)))
            {
                _logger.LogInformation("Missing creditable entities, refreshing them.");
                creditableEntitiesLookup = await GetCreditableEntitiesAsync(platform.Id);
            }

            var itemActions = images.Results.Select(PutItemActionModel (image) =>
            {
                var itemId = $"{platform.Prefix}_{image.IdOnPlatform}";
                if (image.IsDeleted)
                {
                    return new PutDeleteItemActionModelBuilder()
                        .WithItemId(itemId)
                        .Build();
                }

                var indexItemActionBuilder = new PutIndexItemActionModelBuilder()
                    .WithItemId(itemId)
                    .WithPriority(image.ChangeId)
                    .WithImage(image.Thumbnail!.Width, image.Thumbnail.Height, image.Thumbnail.Location)
                    .WithUrl(image.ViewLocation)
                    .WithIsSfw(image.IsSfw);

                foreach (var creditableEntityId in image.Credits)
                {
                    var creditableEntity = creditableEntitiesLookup[creditableEntityId];
                    indexItemActionBuilder.WithAuthor(creditableEntity.IdOnPlatform, creditableEntity.Name);
                }

                return indexItemActionBuilder.Build();
            }).ToList();

            _logger.LogInformation("Ingesting {Count} items.", itemActions.Count);
            await _ingestionApiClient.PutItemActionsAsync(itemActions);

            changeId = images.NextChangeId;

            state.Platforms[platform.Id] = changeId;
            await _stateRepository.PutAsync(state);
        }

        _logger.LogInformation(cancellationToken.IsCancellationRequested ? "Ingestion cancelled." : "Done ingesting.");
    }

    private async Task<Dictionary<int, CreditableEntityModel>> GetCreditableEntitiesAsync(string platform)
    {
        var lookup = new Dictionary<int, CreditableEntityModel>();

        var changeId = 0L;
        while (true)
        {
            var creditableEntities = await _mainApiClient.GetCreditableEntitiesAsync(platform, changeId);
            if (creditableEntities.Results.Count == 0)
            {
                break;
            }

            foreach (var result in creditableEntities.Results)
            {
                lookup[result.Id] = result;
            }

            changeId = creditableEntities.NextChangeId;
        }

        return lookup;
    }
}
