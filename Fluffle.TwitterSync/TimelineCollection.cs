using Humanizer;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Serilog;
using SerilogTimings;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Noppes.Fluffle.TwitterSync
{
    public class TimelineCollection : IEnumerable<ITweet>
    {
        private readonly ITwitterClient _twitterClient;
        private readonly TweetRetriever _tweetRetriever;
        private readonly IUser _user;

        private IDictionary<string, ITweet> _tweets;

        private TimelineCollection(ITwitterClient twitterClient, TweetRetriever tweetRetriever, IUser user)
        {
            _twitterClient = twitterClient;
            _tweetRetriever = tweetRetriever;
            _user = user;
        }

        public static async Task<TimelineCollection> CreateAsync(ITwitterClient twitterClient, TweetRetriever tweetRetriever, IUser user, ImmutableHashSet<string> existingTweets = null)
        {
            var collection = new TimelineCollection(twitterClient, tweetRetriever, user);
            await collection.FillWithTimelineAsync(existingTweets);

            return collection;
        }

        public ITweet GetReplyRootTweet(ITweet tweet)
        {
            if (tweet == null)
                return null;

            if (tweet.Type() != TweetType.Reply)
                return tweet;

            return _tweets.TryGetValue(tweet.InReplyToStatusIdStr, out var replyTweet)
                ? GetReplyRootTweet(replyTweet)
                : null;
        }

        private async Task FillWithTimelineAsync(ImmutableHashSet<string> existingTweets)
        {
            var pageCounter = 1;
            var tweets = new List<ITweet>();
            var iterator = _twitterClient.Timelines.GetUserTimelineIterator(new GetUserTimelineParameters(_user.Id)
            {
                IncludeRetweets = true
            });
            while (!iterator.Completed)
            {
                using var _ = Operation.Time("Retrieved {page} page of tweets for user @{username}", pageCounter.Ordinalize(), _user.ScreenName);
                var page = await HttpResiliency.RunAsync(() => iterator.NextPageAsync());
                tweets.AddRange(page);

                var pageIds = page.Select(t => t.IdStr);
                if (existingTweets != null && existingTweets.Intersect(pageIds).Count != 0)
                    break;

                pageCounter++;
            }

            if (existingTweets != null)
            {
                tweets = tweets
                    .Where(x => (x.CreatedBy.IdStr == _user.IdStr && x.Type() == TweetType.Post) || !existingTweets.Contains(x.IdStr))
                    .ToList();
            }

            _tweets = tweets.Flatten().ToDictionary(t => t.IdStr);
        }

        public async Task FillMissingAsync(int maxDepth)
        {
            var priority = await _tweetRetriever.AcquirePriorityAsync();

            var depth = 0;
            while (true)
            {
                depth++;

                var missingIds = this
                    .Where(t => t.Type() == TweetType.Reply)
                    .Select(t => t.InReplyToStatusIdStr)
                    .ToHashSet();
                missingIds.ExceptWith(_tweets.Keys);

                Log.Information("Missing {count} tweets for user @{username} at depth {depth}", missingIds.Count, _user.ScreenName, depth);
                if (missingIds.Count == 0)
                    break;

                var retrievedMissing = await _tweetRetriever.GetTweets(priority, missingIds.Select(long.Parse).ToList());
                foreach (var missingId in missingIds)
                {
                    _tweets.Add(missingId, retrievedMissing.Find(t => t.IdStr == missingId));
                }

                if (depth >= maxDepth)
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ITweet> GetEnumerator() => _tweets.Values.Where(t => t != null).GetEnumerator();
    }
}
