using Flurl.Http;
using MongoDB.Driver;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Core;
using Noppes.Fluffle.Twitter.Core.Services;
using Noppes.Fluffle.Twitter.Database;
using Noppes.Fluffle.Utils;
using Serilog;

namespace Noppes.Fluffle.Twitter.FurryChecker;

internal class Program : QueuePollingService<Program, UserCheckFurryQueueItem>
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    protected override TimeSpan VisibleAfter => TimeSpan.FromHours(1);

    private const int NMediaToRetrieve = 100;
    private const int NMediaToAnalyze = 10;

    private static async Task Main(string[] args) => await RunAsync(args, "TwitterFurryChecker", (conf, services) =>
    {
        services.AddCore(conf);
    });

    private readonly IUserService _userService;
    private readonly TwitterContext _twitterContext;
    private readonly ITwitterApiClient _twitterApiClient;
    private readonly IFluffleMachineLearningApiClient _mlClient;
    private readonly IQueue<UserCheckFurryQueueItem> _queue;

    public Program(IServiceProvider services, IUserService userService, TwitterContext twitterContext, ITwitterApiClient twitterApiClient, IFluffleMachineLearningApiClient mlClient, IQueue<UserCheckFurryQueueItem> queue) : base(services)
    {
        _userService = userService;
        _twitterContext = twitterContext;
        _twitterApiClient = twitterApiClient;
        _mlClient = mlClient;
        _queue = queue;
    }

    public override async Task ProcessAsync(UserCheckFurryQueueItem value, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(value.Id);
        if (user == null)
            return;

        var photoMedias = await GetPhotoMediaAsync(user);

        var temporaryFiles = new List<TemporaryFile>();
        try
        {
            var mediaUsed = new List<(TwitterTweetMediaModel media, TwitterTweetMediaPhotoModel photo)>();
            foreach (var photoMedia in photoMedias)
            {
                // Download the largest photo
                var photo = photoMedia.Photos!
                    .OrderByDescending(x => x.Width * x.Height)
                    .First();

                await using var stream = await TryDownloadImageAsync(photo.Url);
                if (stream == null)
                    continue;

                var temporaryFile = new TemporaryFile();
                try
                {
                    // Flush downloaded stream to temporary file
                    await using (var temporaryFileStream = temporaryFile.OpenFileStream())
                        await stream.CopyToAsync(temporaryFileStream);

                    // Verify the downloaded data is actually a valid image (yes there are invalid images on Twitter)
                    await using (var temporaryFileStream = temporaryFile.OpenFileStream())
                    {
                        var isValid = await _mlClient.VerifyImageAsync(temporaryFileStream);
                        if (!isValid)
                            continue;
                    }

                    temporaryFiles.Add(temporaryFile);
                    mediaUsed.Add((photoMedia, photo));
                }
                catch
                {
                    temporaryFile.Dispose();
                    throw;
                }

                if (temporaryFiles.Count == NMediaToAnalyze)
                    break;
            }

            if (temporaryFiles.Count < NMediaToAnalyze)
            {
                Log.Information("Not enough media could be retrieved for user @{username}. Rescheduling to check again in about a month", user.Username);
                await _queue.EnqueueAsync(new UserCheckFurryQueueItem
                {
                    Id = user.Id
                }, user.FollowersCount, RandomTimeSpan.Between(TimeSpan.FromDays(26), TimeSpan.FromDays(30)), null);

                return;
            }

            await AnalyzeAndSaveAsync(user, mediaUsed, temporaryFiles);
        }
        finally
        {
            foreach (var temporaryFile in temporaryFiles)
                temporaryFile.Dispose();
        }
    }

    private async Task<Stream?> TryDownloadImageAsync(string url)
    {
        Log.Information("Downloading image at {url}...", url);
        Stream? stream = null;
        try
        {
            stream = await _twitterApiClient.GetStreamAsync(url, true);
        }
        catch (FlurlHttpException e)
        {
            if (stream != null)
                await stream.DisposeAsync();

            if (e.StatusCode == 404)
            {
                Log.Warning("Image at {url} could not be found", url);
                return null;
            }

            throw;
        }

        return stream;
    }

    private async Task AnalyzeAndSaveAsync(UserEntity user, IList<(TwitterTweetMediaModel media, TwitterTweetMediaPhotoModel photo)> mediaUsed, IEnumerable<TemporaryFile> temporaryFiles)
    {
        var imageStreams = new List<Stream>();
        try
        {
            foreach (var temporaryFile in temporaryFiles)
                imageStreams.Add(temporaryFile.OpenFileStream());

            Log.Information("Running images through model that determines whether the images contain furry art...");
            var furryArtPredictions = await _mlClient.GetFurryArtPredictionsAsync(imageStreams);

            Log.Information("Running scores through model that determines whether the user is a furry artist...");
            var furryArtistScore = await _mlClient.GetFurryArtistPredictionAsync(furryArtPredictions);
            var isFurryArtist = furryArtistScore > 0.2;

            Log.Information("It was determined that user @{username} {isFurryArtist} a furry artist with a confidence of {score}", user.Username, isFurryArtist ? "is" : "is not", furryArtistScore);

            // Save results to database
            var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, user.Id);
            var update = Builders<UserEntity>.Update.Set(x => x.FurryPrediction, new UserFurryPredictionEntity
            {
                Version = 1,
                Score = furryArtistScore,
                Value = isFurryArtist,
                DeterminedWhen = DateTime.UtcNow,
                ImagesUsed = mediaUsed.Select((x, i) => new UserFurryPredictionImageUsedEntity
                {
                    MediaId = x.media.Id,
                    Url = x.photo.Url,
                    Score = furryArtPredictions[i]
                }).OrderByDescending(x => x.Score).ToList()
            });
            await _twitterContext.Users.Collection.FindOneAndUpdateAsync(filter, update);
        }
        finally
        {
            foreach (var imageStream in imageStreams)
                await imageStream.DisposeAsync();
        }
    }

    private async Task<UserEntity?> GetUserAsync(string id)
    {
        Log.Information("Start processing user with ID {id}", id);
        var user = await _twitterContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            Log.Warning("User with ID {id} couldn't be found in the database", id);
            return null;
        }
        Log.Information("User for ID {id} is @{username}", id, user.Username);

        Log.Information("Updating details for user @{username}...", user.Username);
        user = await _userService.UpdateDetailsAsync(user);
        if (!user.IsActive)
        {
            Log.Information("After updating the details for user @{username}, it was determined the user's media could not be scraped. Rescheduling about a month later to check again", user.Username);
            await _queue.EnqueueAsync(new UserCheckFurryQueueItem
            {
                Id = user.Id
            }, user.FollowersCount, RandomTimeSpan.Between(TimeSpan.FromDays(26), TimeSpan.FromDays(30)), null);

            return null;
        }

        return user;
    }

    private async Task<IList<TwitterTweetMediaModel>> GetPhotoMediaAsync(UserEntity user)
    {
        Log.Information("Start retrieving media for user @{username}...", user.Username);

        var media = new List<(TwitterTweetModel tweet, TwitterTweetMediaModel photo)>();
        await foreach (var tweets in _twitterApiClient.EnumerateUserMediaAsync(user.Id))
        {
            var relevant = tweets.Select(tweet =>
            {
                var photo = tweet.Media.FirstOrDefault(x => x.Type == "photo");
                if (photo == null)
                    return default;

                return (tweet, photo);
            }).Where(x => x != default).Select(x => x).ToList();

            Log.Information("Out of the {tweetCount} tweets retrieved, {usableCount} contained usable photos", tweets.Count, relevant.Count);
            media.AddRange(relevant);

            if (media.Count >= NMediaToRetrieve)
                break;
        }

        var photoMedia = media
            .Take(NMediaToRetrieve) // Take the N most recent
            .OrderByDescending(x => x.tweet.FavoriteCount) // Order by most liked
            .Select(x => x.photo)
            .ToList();

        return photoMedia;
    }
}
