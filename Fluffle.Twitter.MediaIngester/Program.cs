using Flurl.Http;
using MongoDB.Driver;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Core;
using Noppes.Fluffle.Twitter.Database;
using Serilog;
using System.Net;

namespace Noppes.Fluffle.Twitter.MediaIngester;

internal class Program : QueuePollingService<Program, MediaIngestQueueItem>
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    protected override TimeSpan VisibleAfter => TimeSpan.FromHours(6);

    private static readonly FlurlRetryPolicyBuilder DownloadRetryPolicy = new FlurlRetryPolicyBuilder()
        .WithStatusCode(HttpStatusCode.GatewayTimeout)
        .ShouldRetryClientTimeouts(true)
        .ShouldRetryNetworkErrors(true)
        .WithRetry(3, retryCount => TimeSpan.FromSeconds(5 * retryCount));

    private static async Task Main(string[] args) => await RunAsync(args, "TwitterMediaIngester", (conf, services) =>
    {
        services.AddCore(conf);
    });

    private readonly TwitterContext _twitterContext;
    private readonly ITwitterApiClient _twitterApiClient;
    private readonly IFluffleMachineLearningApiClient _mlApiClient;
    private readonly FluffleClient _fluffleClient;
    private readonly IQueue<MediaIngestQueueItem> _queue;

    public Program(IServiceProvider services, TwitterContext twitterContext, ITwitterApiClient twitterApiClient,
        IFluffleMachineLearningApiClient mlApiClient, FluffleClient fluffleClient, IQueue<MediaIngestQueueItem> queue) : base(services)
    {
        _twitterContext = twitterContext;
        _twitterApiClient = twitterApiClient;
        _mlApiClient = mlApiClient;
        _fluffleClient = fluffleClient;
        _queue = queue;
    }

    public override async Task ProcessAsync(MediaIngestQueueItem value, CancellationToken cancellationToken)
    {
        Log.Information("Start processing media with ID {mediaId} for tweet with ID {tweetId}", value.MediaId, value.TweetId);

        var tweet = await _twitterContext.Tweets.FirstOrDefaultAsync(x => x.Id == value.TweetId);
        if (tweet == null)
        {
            Log.Warning("No tweet could be found with ID {id}", value.TweetId);
            return;
        }
        var user = await _twitterContext.Users.FirstAsync(x => x.Id == tweet.UserId);
        Log.Information("Tweet with ID {tweetId} is owned by @{username}", tweet.Id, user.Username);

        var media = tweet.Media.FirstOrDefault(x => x.Id == value.MediaId);
        if (media == null)
        {
            Log.Warning("No media could be found with ID {mediaId} for tweet with ID {tweetId}", value.MediaId, value.TweetId);
            return;
        }

        if (media.Type != "photo")
        {
            Log.Warning("Media with ID {id} could not be processed because it isn't of type photo", value.MediaId);
            return;
        }

        var mostPopularTweetWithMedia = await GetMostPopularTweetWithMediaAsync(media.Id);
        if (mostPopularTweetWithMedia.Id != tweet.Id)
        {
            Log.Information("Skipping processing because there is a tweet with the same media that is more popular");
            return;
        }

        var photos = media.Photos!
            .OrderByDescending(x => x.Width * x.Height)
            .ToList();

        foreach (var photo in photos)
        {
            Stream? stream = null;
            try
            {
                stream = await DownloadRetryPolicy.Execute(() => _twitterApiClient.GetStreamAsync(photo.Url));
                var furryArtScores = await _mlApiClient.GetFurryArtPredictionsAsync(new[] { stream });
                var furryArtScore = furryArtScores.First();
                var isFurryArt = furryArtScore > 0.2;

                Log.Information("Is media with ID {id} furry art: {isFurryArt}, {furryArtScore}", media.Id, isFurryArt, furryArtScore);

                var idx = tweet.Media.IndexOf(media);
                var filter = Builders<TweetEntity>.Filter.Eq(x => x.Id, tweet.Id);
                var update = Builders<TweetEntity>.Update.Set(x => x.Media[idx].FurryPrediction, new TweetMediaFurryPredictionEntity
                {
                    Version = 1,
                    Score = furryArtScore,
                    Value = isFurryArt,
                    DeterminedWhen = DateTime.UtcNow
                });
                await _twitterContext.Tweets.Collection.FindOneAndUpdateAsync(filter, update);

                // Submit the media to Fluffle if it is furry art
                if (!isFurryArt)
                    return;

                var model = CreateContentModel(user, tweet, media);
                await HttpResiliency.RunAsync(async () => await _fluffleClient.PutContentAsync("Twitter", new[]
                {
                    model
                }));

                return;
            }
            catch (FlurlHttpException e)
            {
                if (e.StatusCode == 403)
                {
                    Log.Warning("Media with ID {id} couldn't be retrieved because a 403 Forbidden was returned. Putting it on queue to be checked again in 7 days.", media.Id);
                    await _queue.EnqueueAsync(value, user.FollowersCount, TimeSpan.FromDays(7), null);
                    return;
                }

                if (e.StatusCode == 404)
                    continue;

                throw;
            }
            finally
            {
                if (stream != null)
                    await stream.DisposeAsync();
            }
        }

        Log.Warning("No photos could be processed for media with ID {id}", media.Id);
    }

    private async Task<TweetEntity> GetMostPopularTweetWithMediaAsync(string mediaId)
    {
        var filter = Builders<TweetEntity>.Filter.ElemMatch(x => x.Media, x => x.Id == mediaId);
        var sort = Builders<TweetEntity>.Sort.Descending(x => x.LikeCount);
        var cursor = await _twitterContext.Tweets.Collection.FindAsync(filter, new FindOptions<TweetEntity>
        {
            Sort = sort,
        });
        var tweet = await cursor.FirstAsync();

        return tweet;
    }

    private static PutContentModel CreateContentModel(UserEntity user, TweetEntity tweet, TweetMediaEntity media)
    {
        return new PutContentModel
        {
            IdOnPlatform = media.Id,
            Reference = null,
            ViewLocation = $"https://twitter.com/{user.Username}/status/{tweet.Id}",
            Title = null,
            Description = tweet.Text,
            Rating = ContentRatingConstant.Explicit,
            MediaType = MediaTypeConstant.Image,
            Tags = null,
            Priority = user.FollowersCount,
            Files = media.Photos!.Select(x => new PutContentModel.FileModel
            {
                Width = x.Width,
                Height = x.Height,
                Location = x.Url,
                Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(new Uri(x.Url).AbsolutePath)),
            }).ToList(),
            CreditableEntities = new List<PutContentModel.CreditableEntityModel>
            {
                new()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Type = CreditableEntityType.Owner
                }
            },
            OtherSources = null,
            Source = null,
            SourceVersion = null,
            ShouldBeIndexed = true
        };
    }
}