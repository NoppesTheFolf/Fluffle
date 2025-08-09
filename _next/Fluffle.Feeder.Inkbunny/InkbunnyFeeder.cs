using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Inkbunny.Client;
using Fluffle.Feeder.Inkbunny.Client.Models;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using System.Collections.Immutable;
using System.Globalization;

namespace Fluffle.Feeder.Inkbunny;

internal class InkbunnyFeeder
{
    private const int BatchSize = 100;

    private readonly InkbunnyClient _inkbunnyClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<InkbunnyFeederState> _stateRepository;
    private readonly ILogger _logger;

    public InkbunnyFeeder(
        InkbunnyClient inkbunnyClient,
        IIngestionApiClient ingestionApiClient,
        IStateRepository<InkbunnyFeederState> stateRepository,
        ILogger logger)
    {
        _inkbunnyClient = inkbunnyClient;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepository;
        _logger = logger;
    }

    public async Task RunAsync(InkbunnyFeederState state, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (state.CurrentId < 1)
            {
                _logger.LogInformation("Exiting because there are no more submissions left to retrieve.");
                break;
            }

            var idsInt = Enumerable.Range(state.CurrentId - BatchSize + 1, BatchSize).Where(x => x > 0).ToList();
            var ids = idsInt
                .Select(x => x.ToString(CultureInfo.InvariantCulture))
                .ToImmutableHashSet();

            _logger.LogInformation("Start retrieving submissions between ID {StartId} and ID {EndId}.", idsInt.Min(), idsInt.Max());
            var response = await _inkbunnyClient.GetSubmissionsAsync(ids);
            _logger.LogInformation("Retrieved {Count} submissions.", response.Submissions.Count);

            var itemActions = new List<PutItemActionModel>();

            var retrievedSubmissionIds = response.Submissions.Select(x => x.Id).ToImmutableHashSet();
            var missingSubmissionIds = ids.Except(retrievedSubmissionIds);
            foreach (var missingSubmissionId in missingSubmissionIds)
            {
                var deleteItemAction = new PutDeleteGroupItemActionModelBuilder()
                    .WithGroupId($"inkbunny_{missingSubmissionId}")
                    .Build();

                itemActions.Add(deleteItemAction);
            }

            foreach (var submission in response.Submissions)
            {
                var groupBuilder = new GroupedPutItemActionModelBuilder($"inkbunny_{submission.Id}");
                for (var i = 0; i < submission.Files.Count; i++)
                {
                    var file = submission.Files[i];
                    var extension = Path.GetExtension(file.FullFileUrl);
                    if (!ImageHelper.IsSupportedExtension(extension))
                    {
                        continue;
                    }

                    var url = $"https://inkbunny.net/s/{submission.Id}";
                    if (i != 0)
                    {
                        url += $"-p{i + 1}";
                    }

                    // This shouldn't really happen, but it might for broken submissions.
                    // Example where width/height are missing: https://inkbunny.net/s/2937680.
                    var images = file.GetImages().ToList();
                    if (images.Count == 0)
                    {
                        continue;
                    }

                    groupBuilder.AddItem()
                        .WithItemId($"inkbunny_{submission.Id}-{file.Id}")
                        .WithPriority(submission.CreatedWhen)
                        .WithUrl(url)
                        .WithImages(images)
                        .WithAuthor(submission.UserId, submission.Username)
                        .WithIsSfw(submission.Rating == InkbunnySubmissionRating.General);
                }

                var groupItemActions = groupBuilder.Build();
                itemActions.AddRange(groupItemActions);
            }

            await _ingestionApiClient.PutItemActionsAsync(itemActions);

            state.CurrentId -= BatchSize;
            await _stateRepository.PutAsync(state);

            var oldestSubmissions = response.Submissions.MinBy(x => x.CreatedWhen);
            if (oldestSubmissions == null)
            {
                continue;
            }

            if (oldestSubmissions.CreatedWhen.UtcDateTime < state.RetrieveUntil)
            {
                _logger.LogInformation("Exiting because the oldest retrieved submission has a date smaller than the stopping date.");
                break;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
    }
}
