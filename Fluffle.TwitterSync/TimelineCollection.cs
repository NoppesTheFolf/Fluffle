using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Serilog;
using SerilogTimings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using static MoreLinq.Extensions.BatchExtension;

namespace Noppes.Fluffle.TwitterSync
{
    public class TimelineCollection : IEnumerable<ITweet>
    {
        private const int BatchSize = 100;

        private readonly ITwitterClient _twitterClient;
        private readonly IUser _user;

        private IDictionary<string, ITweet> _tweets;

        private TimelineCollection(ITwitterClient twitterClient, IUser user)
        {
            _twitterClient = twitterClient;
            _user = user;
        }

        public static async Task<TimelineCollection> CreateAsync(ITwitterClient twitterClient, IUser user)
        {
            var collection = new TimelineCollection(twitterClient, user);
            await collection.FillWithTimelineAsync();

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

        private async Task FillWithTimelineAsync()
        {
            var tweets = new List<ITweet>();
            var iterator = _twitterClient.Timelines.GetUserTimelineIterator(new GetUserTimelineParameters(_user.Id)
            {
                IncludeRetweets = true
            });
            while (!iterator.Completed)
            {
                using var _ = Operation.Time("Retrieved {count} tweets for user @{username}", tweets.Count, _user.ScreenName);
                var page = await HttpResiliency.RunAsync(() => iterator.NextPageAsync());
                tweets.AddRange(page);
            }

            _tweets = tweets.Flatten().ToDictionary(t => t.IdStr);
        }

        public async Task FillMissingAsync()
        {
            while (true)
            {
                var missingIds = this
                    .Where(t => t.Type() == TweetType.Reply)
                    .Select(t => t.InReplyToStatusIdStr)
                    .ToHashSet();
                missingIds.ExceptWith(_tweets.Keys);

                Log.Information("Missing {count} tweets for user @{username}", missingIds.Count, _user.ScreenName);
                if (missingIds.Count == 0)
                    break;

                var retrievedMissing = new List<ITweet>();
                foreach (var batch in missingIds.Select(long.Parse).Batch(BatchSize).Select(b => b.ToArray()))
                {
                    using var _ = Operation.Time("Retrieving {count} missing tweets for user @{username}", batch.Length, _user.ScreenName);
                    var retrievedBatch = await HttpResiliency.RunAsync(() => _twitterClient.Tweets.GetTweetsAsync(batch));

                    retrievedMissing.AddRange(retrievedBatch);
                }

                foreach (var missingId in missingIds)
                {
                    _tweets.Add(missingId, retrievedMissing.Find(t => t.IdStr == missingId));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ITweet> GetEnumerator() => _tweets.Values.Where(t => t != null).GetEnumerator();
    }
}
