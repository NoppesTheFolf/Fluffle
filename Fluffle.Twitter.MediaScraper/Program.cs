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

    private async Task CalculateTweetsPerDayAsync(UserEntity user)
    {
        const double nDaysToConsider = 180;

        if (user.MediaLastScrapedAt == null)
            throw new InvalidOperationException("Tweets per day statistics cannot be calculated for users of which their media hasn't been scraped before.");

        var mediaLastScrapedAt = (DateTime)user.MediaLastScrapedAt;
        var maximumTweetAge = mediaLastScrapedAt.Subtract(TimeSpan.FromDays(nDaysToConsider));

        var userIdFilter = Builders<TweetEntity>.Filter.Eq(x => x.UserId, user.Id);
        var ageFilter = Builders<TweetEntity>.Filter.Gt(x => x.CreatedAt, maximumTweetAge);
        var filter = userIdFilter & ageFilter;

        var nTweets = await _twitterContext.Tweets.Collection.Find(filter).CountDocumentsAsync();
        var tweetsPerDay = nTweets / nDaysToConsider;

        user.TweetsPerDay = tweetsPerDay;
        user.TweetsPerDayBasedOnWhen = user.MediaLastScrapedAt;
        await UpdateTweetsPerDayAsync(user.Id, tweetsPerDay, mediaLastScrapedAt);
    }

    private async Task UpdateTweetsPerDayAsync(string userId, double tweetsPerDay, DateTime basedOnWhen)
    {
        var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, userId);
        var update = Builders<UserEntity>.Update
            .Set(x => x.TweetsPerDay, tweetsPerDay)
            .Set(x => x.TweetsPerDayBasedOnWhen, basedOnWhen);

        await _twitterContext.Users.Collection.UpdateOneAsync(filter, update);
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
                    .Where(x => x.CanMediaBeRetrieved) // Of which the media can be retrieved
                    .Where(x => x.MediaScrapingLastStartedAt == null || DateTime.UtcNow.Subtract((DateTime)x.MediaScrapingLastStartedAt) > TimeSpan.FromHours(3)) // Do not include users that might have caused the scraper to crash before
                    .ToList();

                var usersWithMissingStatistics = users
                    .Where(x => x.MediaLastScrapedAt != null) // Only include users which has their media scraped before
                    .Where(x => x.TweetsPerDay == null || x.MediaLastScrapedAt != x.TweetsPerDayBasedOnWhen)
                    .ToList();

                foreach (var user in usersWithMissingStatistics)
                {
                    Log.Information("Updating tweets per day statistic for @{username}", user.Username);
                    await CalculateTweetsPerDayAsync(user);
                }

                var orderedUsers = users
                    .Select(x => (order: CalculateScrapeOrder(x), user: x))
                    .Where(x => x.order != null)
                    .OrderBy(x => x.order) // Because we're using a stack, ascending order ends up descending order!
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

    private static double CalculateTweetWeight(UserEntity user)
    {
        var kFollowers = user.FollowersCount / 1000d;
        var weight = Math.Max(1, kFollowers);

        var cappedWeight = Math.Min(10, weight);
        var logWeight = Math.Log(weight);
        var finalWeight = cappedWeight + logWeight;

        return finalWeight;
    }

    private static double? CalculateScrapeOrder(UserEntity user)
    {
        if (user.MediaLastScrapedAt == null)
            return int.MaxValue; // Prioritize newly imported users to the top, prioritize by popularity

        var timeSinceMediaLastScraped = DateTime.UtcNow.Subtract((DateTime)user.MediaLastScrapedAt);
        if (timeSinceMediaLastScraped < TimeSpan.FromDays(1))
            return null; // Do not scrape the media of users who have already been scraped in the last 24 hours

        var tweetWeight = CalculateTweetWeight(user);
        var expectedNumberOfMissingTweets = timeSinceMediaLastScraped.TotalDays * user.TweetsPerDay;
        var order = expectedNumberOfMissingTweets * tweetWeight;

        return order;
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
            Log.Information("So far {tweetCount} have been retrieved for user @{username}", tweets.Count, user.Username);

            // If less new tweets were retrieved than the full page size, then we know we've started
            // retrieving tweets that are already in the database
            if (tweetsPage.Count != newTweets.Count)
                break;
        }

        if (tweets.Count == 0)
        {
            Log.Information("No new tweets were retrieved for @{username}", user.Username);

            // Update when the user last got scraped
            await UpdateMediaLastScrapedAtAsync(user);

            return;
        }
        Log.Information("Retrieved a total of {count} (new) tweets for @{username}", tweets.Count, user.Username);

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
            CreatedAt = x.CreatedAt.ToUniversalTime(),
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

        if (queueItems.Count != 0)
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
