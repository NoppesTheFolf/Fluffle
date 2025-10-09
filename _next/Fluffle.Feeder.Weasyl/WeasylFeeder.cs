using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Weasyl.ApiClient;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Feeder.Weasyl;

internal class WeasylFeeder
{
    private readonly WeasylApiClient _weasylApiClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<WeasylFeederState> _stateRepository;
    private readonly ILogger _logger;

    public WeasylFeeder(
        WeasylApiClient weasylApiClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepository<WeasylFeederState> stateRepository,
        ILogger logger)
    {
        _weasylApiClient = weasylApiClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepository;
        _logger = logger;
    }

    public async Task RunAsync(WeasylFeederState state, CancellationToken cancellationToken)
    {
        for (var i = state.CurrentSubmissionId; i > state.EndSubmissionId; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Start retrieving submission with ID {Id}.", i);
            var submission = await _weasylApiClient.GetSubmissionAsync(i, anyway: true);
            var submissionImages = submission?.GetImages().ToList();
            _logger.LogInformation("Retrieved submission with ID {Id}.", i);

            var itemId = $"weasyl_{i}";
            PutItemActionModel itemAction;
            if (submission == null || submission.Subtype != WeasylSubmissionSubtype.Visual || submissionImages == null || submissionImages.Count == 0)
            {
                _logger.LogInformation("Marking submission with ID {Id} to be deleted.", i);

                itemAction = new PutDeleteItemActionModelBuilder()
                    .WithItemId(itemId)
                    .Build();
            }
            else
            {
                _logger.LogInformation("Indexing submission with ID {Id}.", i);

                itemAction = new PutIndexItemActionModelBuilder()
                    .WithItemId(itemId)
                    .WithCreatedWhen(submission.PostedAt)
                    .WithUrl($"https://weasyl.com/submission/{submission.SubmitId}")
                    .WithImages(submissionImages)
                    .WithIsSfw(submission.Rating == WeasylSubmissionRating.General)
                    .WithAuthor(submission.OwnerLogin, submission.Owner)
                    .Build();
            }

            await _ingestionApiClient.PutItemActionsAsync([itemAction]);

            state.CurrentSubmissionId = i - 1;
            await _stateRepository.PutAsync(state);
        }

        state.EndSubmissionId = state.StartSubmissionId;
        await _stateRepository.PutAsync(state);
    }
}
