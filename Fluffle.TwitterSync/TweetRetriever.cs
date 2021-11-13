using Nito.AsyncEx;
using Noppes.Fluffle.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace Noppes.Fluffle.TwitterSync
{
    public class TweetRetriever
    {
        private const int BatchSize = 100;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15 * 60 / (double)300);

        private readonly ITwitterClient _twitterClient;
        private readonly AsyncLock _mutex;
        private readonly Dictionary<long, IList<TweetRetrieverRequest>> _requests;
        private DateTimeOffset _waitUntil;

        public TweetRetriever(ITwitterClient twitterClient)
        {
            _twitterClient = twitterClient;
            _mutex = new AsyncLock();
            _requests = new Dictionary<long, IList<TweetRetrieverRequest>>();
            _waitUntil = DateTimeOffset.UtcNow;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                _waitUntil = DateTimeOffset.UtcNow.Add(Interval);
                await ProcessAsync();

                var timeToWait = _waitUntil.Subtract(DateTimeOffset.UtcNow);
                if (timeToWait > TimeSpan.Zero)
                    await Task.Delay(Interval);
            }
        }

        private async Task ProcessAsync()
        {
            using var _ = await _mutex.LockAsync();

            if (_requests.Count == 0)
                return;

            var batch = _requests.Take(BatchSize).ToList();
            var batchIds = batch.Select(x => x.Key).ToArray();
            Log.Information("Retrieving {count} out of {totalCount} tweets by ID", batchIds.Length, _requests.Count);
            var retrievedTweets = await HttpResiliency.RunAsync(() => _twitterClient.Tweets.GetTweetsAsync(batchIds));
            var retrievedTweetsLookup = retrievedTweets.ToDictionary(t => t.Id);

            foreach (var (tweetId, requests) in batch)
            {
                retrievedTweetsLookup.TryGetValue(tweetId, out var retrievedTweet);

                foreach (var request in requests)
                {
                    request.ToProcess.Remove(tweetId);

                    if (retrievedTweet != null)
                        request.Retrieved.Add(retrievedTweet);

                    if (request.ToProcess.Count == 0)
                        request.CompletionNotifier.Set();
                }

                _requests.Remove(tweetId);
            }
        }

        public async Task<List<ITweet>> GetTweets(IEnumerable<long> tweetIds)
        {
            var request = await EnqueueAsync(tweetIds);
            await request.CompletionNotifier.WaitAsync();

            return request.Retrieved;
        }

        private async Task<TweetRetrieverRequest> EnqueueAsync(IEnumerable<long> tweetIds)
        {
            var request = new TweetRetrieverRequest(tweetIds);

            using var _ = await _mutex.LockAsync();
            foreach (var tweetId in request.ToProcess)
            {
                if (_requests.TryGetValue(tweetId, out var requests))
                {
                    requests.Add(request);
                    continue;
                }

                _requests.Add(tweetId, new List<TweetRetrieverRequest> { request });
            }

            return request;
        }

        private class TweetRetrieverRequest
        {
            public AsyncManualResetEvent CompletionNotifier { get; set; }

            public List<long> ToProcess { get; set; }

            public List<ITweet> Retrieved { get; set; }

            public TweetRetrieverRequest(IEnumerable<long> toProcess)
            {
                CompletionNotifier = new AsyncManualResetEvent();
                ToProcess = toProcess.ToList();
                Retrieved = new List<ITweet>();
            }
        }
    }
}
