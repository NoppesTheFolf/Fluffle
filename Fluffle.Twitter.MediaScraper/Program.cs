using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Core;
using Noppes.Fluffle.Twitter.Core.Services;
using Noppes.Fluffle.Twitter.Database;
using Serilog;

namespace Noppes.Fluffle.Twitter.MediaScraper;

internal class Program : ScheduledService<Program>
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    private static async Task Main(string[] args) => await RunAsync(args, "TwitterMediaScraper", (conf, services) =>
    {
        services.AddCore(conf);
    });

    private DateTime? _refreshedAt;
    private Stack<(double order, UserEntity user)> _users = null!;

    private readonly TwitterContext _twitterContext;
    private readonly ITwitterApiClient _twitterApiClient;
    private readonly IUserService _userService;
    private readonly IQueue<MediaIngestQueueItem> _mediaIngestionQueue;

    public Program(IServiceProvider services, TwitterContext twitterContext, ITwitterApiClient twitterApiClient, IUserService userService, IQueue<MediaIngestQueueItem> mediaIngestionQueue) : base(services)
    {
        _twitterContext = twitterContext;
        _twitterApiClient = twitterApiClient;
        _userService = userService;
        _mediaIngestionQueue = mediaIngestionQueue;
    }

    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (_refreshedAt == null || DateTime.UtcNow.Subtract((DateTime)_refreshedAt) > TimeSpan.FromMinutes(30))
            {
                Log.Information("Start refreshing user list...");

                var users = await _twitterContext.Users.Collection.Find(FilterDefinition<UserEntity>.Empty).ToListAsync();
                users = users
                    .Where(x => x.FurryPrediction?.Value == true) // Users which post furry art
                    .Where(x => !x.IsDeleted) // Of which the account is not deleted
                    .Where(x => x.MediaScrapingLastStartedAt == null || DateTime.UtcNow.Subtract((DateTime)x.MediaScrapingLastStartedAt) > TimeSpan.FromHours(3))
                    .ToList();

                var orderedUsers = users
                    .Select(x => (order: CalculateScrapeOrder(x), user: x))
                    .Where(x => x.order != null)
                    .OrderBy(x => x.order)
                    .Select(x => ((double)x.order!, x.user))
                    .ToList();

                _users = new Stack<(double order, UserEntity user)>(orderedUsers);
                _refreshedAt = DateTime.UtcNow;

                Log.Information("Done refreshing user list! A total of {count} users are scheduled for scraping", _users.Count);
            }

            if (!_users.TryPop(out var tuple))
            {
                Log.Information("Ran out of users to process!");
                break;
            }

            await ProcessUserAsync(tuple.user);
        }
    }

    private static double? CalculateScrapeOrder(UserEntity user)
    {
        if (user.MediaLastScrapedAt == null)
            return 10 + user.FollowersCount; // Prioritize newly imported users to the top, prioritize by popularity

        var timeSinceMediaLastScraped = DateTime.UtcNow.Subtract((DateTime)user.MediaLastScrapedAt);
        if (timeSinceMediaLastScraped < TimeSpan.FromDays(1))
            return null; // Do not scrape the media of users who have already been scraped yesterday

        var followersWeight = user.FollowersCount / (double)10_000;
        if (followersWeight > 3)
            followersWeight = 3; // Cap weight for followers at 3 (or 30_000 followers)

        var timeSinceMediaLastScrapedWeight = timeSinceMediaLastScraped.TotalDays;
        if (timeSinceMediaLastScrapedWeight > 6)
            timeSinceMediaLastScrapedWeight = 6; // Cap weight for last scraped at 6

        return followersWeight + timeSinceMediaLastScrapedWeight;
    }

    private async Task ProcessUserAsync(UserEntity user)
    {
        Log.Information("Starting to scrape media for user @{username}", user.Username);
        await UpdateMediaScrapingLastStartedAtAsync(user);

        user = await _userService.UpdateDetailsAsync(user);
        if (!user.CanMediaBeRetrieved)
        {
            Log.Information("After updating the details for user @{username}, it was determined the user's media could not be scraped", user.Username);
            return;
        }

        var existingTweetsIds = await GetExistingTweetIdsAsync(user.Id);

        var tweets = new List<TwitterTweetModel>();
        await foreach (var tweetsPage in _twitterApiClient.EnumerateUserMediaAsync(user.Id))
        {
            Log.Information("Retrieved {tweetCount} tweets for user @{username}", tweetsPage.Count, user.Username);
            var newTweets = tweetsPage
                .Where(x => !existingTweetsIds.Contains(x.Id))
                .ToList();

            tweets.AddRange(newTweets);

            // If less new tweets were retrieved than the full page size, then we know we've started
            // retrieving tweets that are already in the database
            if (tweetsPage.Count != newTweets.Count)
            {
                Log.Information("Retrieved tweets that have already been scraped before, stopping");
                break;
            }
        }

        if (!tweets.Any())
        {
            Log.Information("No new tweets were retrieved for @{username}", user.Username);
            return;
        }

        var tweetEntities = tweets.Select(x => new TweetEntity
        {
            Id = x.Id,
            UserId = user.Id,
            Text = x.Text,
            LikeCount = x.FavoriteCount,
            QuoteCount = x.QuoteCount,
            ReplyCount = x.ReplyCount,
            RetweetCount = x.RetweetCount,
            BookmarkCount = x.BookmarkCount,
            CreatedAt = x.CreatedAtParsed.ToUniversalTime(),
            Media = x.Media.Select(y => new TweetMediaEntity
            {
                Id = y.Id,
                Type = y.Type,
                Photos = y.Photos?.Select(z => new TweetMediaPhotoEntity
                {
                    Url = z.Url,
                    Width = z.Width,
                    Height = z.Height,
                    Name = z.Name
                }).ToList(),
                Video = y.Video == null ? null : new TweetMediaVideoEntity
                {
                    ThumbnailUrl = y.Video.ThumbnailUrl,
                    Duration = y.Video.Duration,
                    Variants = y.Video.Variants.Select(z => new TweetMediaVideoVariantEntity
                    {
                        Bitrate = z.Bitrate,
                        ContentType = z.ContentType,
                        Url = z.Url
                    }).ToList()
                }
            }).ToList()
        }).ToList();

        // Schedule the media to be ingested into Fluffle
        var queueItems = tweetEntities
            .SelectMany(x => x.Media.Select(y => (tweet: x, media: y)))
            .Where(x => x.media.Type == "photo")
            .Select(x => new MediaIngestQueueItem
            {
                TweetId = x.tweet.Id,
                MediaId = x.media.Id,
            })
            .ToList();

        if (queueItems.Any())
            await _mediaIngestionQueue.EnqueueManyAsync(queueItems, user.FollowersCount, TimeSpan.FromMinutes(1), null);

        // Finally store the tweets in the database
        await _twitterContext.Tweets.Collection.InsertManyAsync(tweetEntities);

        // Update when the user last got scraped
        await UpdateMediaLastScrapedAtAsync(user);
    }

    private async Task UpdateMediaScrapingLastStartedAtAsync(UserEntity user)
    {
        var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, user.Id);
        var update = Builders<UserEntity>.Update.Set(x => x.MediaScrapingLastStartedAt, DateTime.UtcNow);
        await _twitterContext.Users.Collection.FindOneAndUpdateAsync(filter, update);
    }

    private async Task UpdateMediaLastScrapedAtAsync(UserEntity user)
    {
        var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, user.Id);
        var update = Builders<UserEntity>.Update.Set(x => x.MediaLastScrapedAt, DateTime.UtcNow);
        await _twitterContext.Users.Collection.FindOneAndUpdateAsync(filter, update);
    }

    private async Task<HashSet<string>> GetExistingTweetIdsAsync(string userId)
    {
        var existingTweetsIds = await _twitterContext.Tweets.Collection.AsQueryable()
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .ToListAsync();

        return new HashSet<string>(existingTweetsIds);
    }
}
