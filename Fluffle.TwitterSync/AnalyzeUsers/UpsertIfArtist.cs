using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Random = System.Random;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers;

public static class UpsertIfArtist
{
    public const int NextRetrievalWiggleRoomPercentage = 10;
    public const int NextRetrievalWiggleRoomMin = NextRetrievalWiggleRoomPercentage * -1;
    public const int NextRetrievalWiggleRoomMax = NextRetrievalWiggleRoomPercentage + 1;
    public static readonly IDictionary<int, TimeSpan> NextRetrievalWeights = new Dictionary<int, TimeSpan>
    {
        { -1, 3.Days() },
        { 1901, 1.5.Days() },
        { 6409, 1.Days() },
        { 16222, 12.Hours() }
    };

    public static readonly Random Random = new();
    public static readonly AsyncLock UpsertMutex = new();
}

public class UpsertIfArtist<T> : Consumer<T> where T : IUserTweetsSupplierData
{
    private readonly IServiceProvider _services;

    public UpsertIfArtist(IServiceProvider services)
    {
        _services = services;
    }

    public override async Task<T> ConsumeAsync(T data)
    {
        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();
        var user = await context.Users.FirstAsync(u => u.Id == data.Id);

        var now = DateTimeOffset.UtcNow;
        if (user.IsFurryArtist == true)
        {
            user.TimelineRetrievedAt = data.TimelineRetrievedAt;

            var nextRetrievalIn = UpsertIfArtist.NextRetrievalWeights
                .Where(x => user.FollowersCount > x.Key)
                .OrderByDescending(x => x.Key)
                .First().Value;

            int wiggleRoomPercentage;
            lock (UpsertIfArtist.Random)
                wiggleRoomPercentage = UpsertIfArtist.Random.Next(UpsertIfArtist.NextRetrievalWiggleRoomMin, UpsertIfArtist.NextRetrievalWiggleRoomMax);

            var wiggleRoom = nextRetrievalIn.Multiply(wiggleRoomPercentage / 100.0);
            var offset = nextRetrievalIn.Add(wiggleRoom);

            user.TimelineNextRetrievalAt = now.Add(offset);
            if (data.Timeline.Count() != 0)
                await UpsertTweetsAsync(context, data.Timeline, user.Id, CancellationToken.None);

            await context.SaveChangesAsync();
        }

        return data;
    }

    private static async Task UpsertTweetsAsync(TwitterContext context, TimelineCollection timeline, string artistId, CancellationToken cancellationToken)
    {
        // We need to make sure that two upserts do not happen at the same time. When two or
        // more upserts run concurrently, they might end up trying to create the same
        // entity/entities two or more times, causing the database to give a duplicate key error
        // and therefore resulting in application failure.
        using var _ = await UpsertIfArtist.UpsertMutex.LockAsync();

        // Upsert tweets
        var tweets = timeline
            .Where(t => t.CreatedBy.IdStr == artistId)
            .ToList();

        var newTweets = tweets.Select(t => new Tweet
        {
            Id = t.IdStr,
            Url = t.Url,
            FavoriteCount = t.FavoriteCount,
            RetweetCount = t.RetweetCount,
            ReplyTweetId = t.InReplyToStatusIdStr,
            ReplyUserId = t.InReplyToUserIdStr,
            QuotedTweetId = t.QuotedStatusIdStr,
            RetweetTweetId = t.RetweetedTweet?.IdStr,
            CreatedById = t.CreatedBy.IdStr,
            CreatedAt = t.CreatedAt.ToUniversalTime(),
            Type = t.Type(),
            ShouldBeAnalyzed = t.Media.Any(m =>
            {
                // Skip if the tweet does not have any images
                if (m.MediaType() != MediaTypeConstant.Image)
                    return false;

                // Skip if the tweet is a retweet
                if (t.Type() == TweetType.Retweet)
                    return false;

                // If the tweet is a post or a quote tweet, and said tweet is created by the artist, then analyze it
                if (t.Type() == TweetType.Post || t.Type() == TweetType.QuoteTweet)
                    return true;

                // Now we only have replies left, analyze them if the root tweet still exists and is created by the artist
                var rootTweet = timeline.GetReplyRootTweet(t);
                return rootTweet != null && rootTweet.CreatedBy.IdStr == artistId;
            })
        }).ToList();

        var existingTweets = await context.Tweets
            .Where(t => newTweets.Select(nt => nt.Id).Contains(t.Id))
            .ToListAsync(cancellationToken);

        var tweetsResult = await context.SynchronizeAsync(c => c.Tweets, existingTweets, newTweets,
            (t1, t2) => t1.Id == t2.Id, onUpdateAsync: (src, dest) =>
            {
                dest.Url = src.Url;
                dest.FavoriteCount = src.FavoriteCount;
                dest.RetweetCount = src.RetweetCount;

                // If the image is already in the database, and it has been determined that it
                // should be analyzed, but previously it was not, then we need to set this to true.
                if (src.ShouldBeAnalyzed && !dest.ShouldBeAnalyzed)
                    dest.ShouldBeAnalyzed = true;

                return Task.CompletedTask;
            });
        tweetsResult.Print();

        // Upsert users
        var users = tweets
            .Select(t => t.CreatedBy)
            .DistinctBy(t => t.IdStr)
            .ToList();

        var newUsers = users.Select(u => new User
        {
            Id = u.IdStr,
            Name = u.Name,
            Username = u.ScreenName,
            FollowersCount = u.FollowersCount,
            IsProtected = u.Protected
        }).ToList();

        var existingUsers = await context.Users
            .Where(u => newUsers.Select(x => x.Id).Contains(u.Id))
            .ToListAsync(cancellationToken);

        var usersResult = await context.SynchronizeAsync(c => c.Users, existingUsers, newUsers,
            (u1, u2) => u1.Id == u2.Id, onUpdateAsync: (src, dest) =>
            {
                dest.Name = src.Name;
                dest.Username = src.Username;
                dest.FollowersCount = src.FollowersCount;
                dest.IsProtected = src.IsProtected;

                return Task.CompletedTask;
            });
        usersResult.Print();

        // Upsert media
        var media = tweets
            .SelectMany(t => t.Media)
            .DistinctBy(m => m.IdStr)
            .ToList();

        var newMedia = media.Select(m => new Media
        {
            Id = m.IdStr,
            Url = m.MediaURLHttps,
            MediaType = m.MediaType()
        }).ToList();

        var existingMedia = await context.Media
            .Where(m => newMedia.Select(nm => nm.Id).Contains(m.Id))
            .ToListAsync(cancellationToken);

        var mediaResult = await context.SynchronizeAsync(c => c.Media, existingMedia, newMedia,
            (m1, m2) => m1.Id == m2.Id, onUpdateAsync: (src, dest) =>
            {
                dest.Url = src.Url;
                dest.MediaType = src.MediaType;
                // Note that media should not be set to dest.IsNotAvailable = false here.
                // Sometimes media is corrupt and will be set unavailable because of it.

                return Task.CompletedTask;
            });
        mediaResult.Print();

        // Upsert connection between media and tweet
        var newTweetMedia = tweets.SelectMany(t => t.Media.Select(m => new TweetMedia
        {
            TweetId = t.IdStr,
            MediaId = m.IdStr
        })).DistinctBy(tm => (tm.TweetId, tm.MediaId)).ToList();

        var existingTweetMedia = await context.TweetMedia
            .Where(tm => tweets.Select(t => t.IdStr).Contains(tm.TweetId))
            .ToListAsync(cancellationToken);

        var tweetMediaResult = await context.SynchronizeAsync(c => c.TweetMedia, existingTweetMedia, newTweetMedia,
            (tm1, tm2) => (tm1.TweetId, tm1.MediaId) == (tm2.TweetId, tm2.MediaId));
        tweetMediaResult.Print();

        // Upsert media sizes
        var newMediaSizes = media
            .Where(m => m.MediaType() == MediaTypeConstant.Image)
            .SelectMany(m => m.Sizes.Select(kv => new MediaSize
            {
                MediaId = m.IdStr,
                Size = kv.Size(),
                Width = (int)kv.Value.Width,
                Height = (int)kv.Value.Height,
                ResizeMode = kv.Value.Resize()
            })).DistinctBy(ms => (ms.MediaId, ms.Width, ms.Height, ms.ResizeMode)).ToList();

        var existingMediaSizes = await context.MediaSizes
            .Where(ms => media.Select(m => m.IdStr).Contains(ms.MediaId))
            .ToListAsync(cancellationToken);

        var mediaSizesResult = await context.SynchronizeAsync(c => c.MediaSizes, existingMediaSizes, newMediaSizes,
            (ms1, ms2) => (ms1.MediaId, ms1.Width, ms1.Height, ms1.ResizeMode) == (ms2.MediaId, ms2.Width, ms2.Height, ms2.ResizeMode));
        mediaSizesResult.Print();
    }
}
