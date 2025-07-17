using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.FurAffinity.Client;
using Fluffle.Feeder.FurAffinity.Client.Models;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Feeder.FurAffinity;

internal class FurAffinityFeeder
{
    private readonly FurAffinityClient _furAffinityClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<FurAffinityFeederState> _stateRepository;
    private readonly ILogger _logger;

    public FurAffinityFeeder(
        FurAffinityClient furAffinityClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepository<FurAffinityFeederState> stateRepository,
        ILogger logger)
    {
        _furAffinityClient = furAffinityClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepository;
        _logger = logger;
    }

    public async Task RunUntilAsync(FurAffinityFeederState state, TimeSpan minimumAge, CancellationToken cancellationToken)
    {
        for (var i = state.CurrentId; !cancellationToken.IsCancellationRequested; i++)
        {
            var submission = await ProcessIdAsync(i);
            if (submission != null)
            {
                var submissionAge = DateTimeOffset.UtcNow.Subtract(submission.When);
                if (submissionAge < minimumAge)
                {
                    break;
                }
            }

            state.CurrentId = i + 1;
            await _stateRepository.PutAsync(state);
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public async Task RunDecrementAsync(FurAffinityFeederState state, CancellationToken cancellationToken)
    {
        for (var i = state.CurrentId; i > state.EndId; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ProcessIdAsync(i);

            state.CurrentId = i - 1;
            await _stateRepository.PutAsync(state);
        }
    }

    private async Task<FaSubmission?> ProcessIdAsync(int i)
    {
        _logger.LogInformation("Start retrieving submission with ID {Id}.", i);
        var submission = await _furAffinityClient.GetSubmissionAsync(i);
        _logger.LogInformation("Retrieved submission with ID {Id}.", i);

        var deleteReason = submission.GetDeleteReason();

        var itemId = $"furAffinity_{i}";
        PutItemActionModel itemAction;
        if (deleteReason == null)
        {
            _logger.LogInformation("Indexing submission with ID {Id}.", i);

            itemAction = new PutIndexItemActionModelBuilder()
                .WithItemId(itemId)
                .WithPriority(submission!.When)
                .WithImages(submission.GetImages())
                .WithUrl($"https://www.furaffinity.net/view/{i}")
                .WithIsSfw(submission.Rating == FaSubmissionRating.General)
                .WithAuthor(submission.Owner.Id, submission.Owner.Name)
                .Build();
        }
        else
        {
            _logger.LogInformation("Marking submission with ID {Id} to be deleted. Reason: {Reason}.", i, deleteReason);

            itemAction = new PutDeleteItemActionModelBuilder()
                .WithItemId(itemId)
                .Build();
        }

        await _ingestionApiClient.PutItemActionsAsync([itemAction]);

        return submission;
    }
}
