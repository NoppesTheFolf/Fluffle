using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Noppes.E621;

namespace Fluffle.Feeder.E621;

public class E621Feeder
{
    private readonly IE621Client _e621Client;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IStateRepository<E621FeederState> _stateRepository;
    private readonly ILogger _logger;

    public E621Feeder(
        IE621Client e621Client,
        IIngestionApiClient ingestionApiClient,
        IStateRepository<E621FeederState> stateRepository,
        ILogger logger)
    {
        _e621Client = e621Client;
        _ingestionApiClient = ingestionApiClient;
        _stateRepository = stateRepository;
        _logger = logger;
    }

    public async Task RunAsync(E621FeederState state, DateTime retrieveUntil, CancellationToken cancellationToken)
    {
        if (state.CurrentId == null)
        {
            _logger.LogInformation("Start retrieving most recent post.");
            var mostRecentPosts = await _e621Client.GetPostsAsync(limit: 1);
            var mostRecentPost = mostRecentPosts.Single();
            _logger.LogInformation("Most recent post has ID {Id}.", mostRecentPost.Id);

            state.CurrentId = mostRecentPost.Id + 1; // + 1 so also the most recent post gets retrieved in the loop
        }

        Post? newestPost = null;
        while (newestPost == null || newestPost.CreatedAt.UtcDateTime > retrieveUntil)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Start retrieving posts before ID {Id}.", state.CurrentId.Value);
            var posts = await _e621Client.GetPostsAsync(state.CurrentId.Value, Position.Before, E621Constants.PostsMaximumLimit);
            _logger.LogInformation("Retrieved {Count} posts.", posts.Count);

            if (posts.Count == 0)
            {
                _logger.LogInformation("No more posts to retrieve. Exiting.");
                break;
            }

            var allowedFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "jpg", "png", "gif" };
            var indexablePosts = posts
                .Where(x => !x.Flags.IsDeleted)
                .Where(x => allowedFileExtensions.Contains(x.File!.FileExtension!))
                .ToList();

            var indexablePostIds = indexablePosts.Select(x => x.Id).ToHashSet();
            var minId = posts.Select(x => x.Id).Min();
            var maxId = state.CurrentId.Value - 1;

            var deletedPostIds = new List<int>();
            for (var i = minId; i <= maxId; i++)
            {
                if (!indexablePostIds.Contains(i))
                {
                    deletedPostIds.Add(i);
                }
            }

            var indexModels = indexablePosts.Select(x => new PutIndexItemActionModelBuilder()
                    .WithItemId($"e621_{x.Id}")
                    .WithPriority(x.CreatedAt)
                    .WithUrl($"https://e621.net/posts/{x.Id}")
                    .WithImages(x.GetImages())
                    .WithIsSfw(x.IsSfw())
                    .WithAuthors(x.GetAuthors())
                    .Build())
                .ToList<PutItemActionModel>();

            var deleteModels = deletedPostIds
                .Select(x => new PutDeleteItemActionModelBuilder()
                    .WithItemId($"e621_{x}")
                    .Build())
                .ToList<PutItemActionModel>();

            var models = indexModels.Concat(deleteModels).ToList();
            await _ingestionApiClient.PutItemActionsAsync(models);

            newestPost = posts.OrderBy(x => x.CreatedAt).First();

            state.CurrentId = minId;
            await _stateRepository.PutAsync(state);
        }
    }
}
