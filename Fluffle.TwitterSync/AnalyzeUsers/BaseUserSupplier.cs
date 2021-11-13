using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public abstract class BaseUserSupplier<T> : Producer<T> where T : IUserTweetsSupplierData, new()
    {
        protected abstract TimeSpan ReservationTime { get; }
        protected abstract TimeSpan Interval { get; }

        private readonly IServiceProvider _services;
        private readonly ITwitterClient _twitterClient;
        private readonly TweetRetriever _tweetRetriever;

        protected BaseUserSupplier(IServiceProvider services, ITwitterClient twitterClient, TweetRetriever tweetRetriever)
        {
            _services = services;
            _twitterClient = twitterClient;
            _tweetRetriever = tweetRetriever;
        }

        public override async Task WorkAsync()
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();
            var users = await GetUsersAsync(context);

            if (users.Count == 0)
            {
                Log.Information("Waiting for {interval} before trying to supply users again", Interval);
                await Task.Delay(Interval);
                return;
            }

            IUser twitterUser = null;
            foreach (var user in users)
            {
                var produced = new T();

                try
                {
                    twitterUser = await HttpResiliency.RunAsync(() => _twitterClient.Users.GetUserAsync(long.Parse(user.Id)));

                    user.ReservedUntil = DateTimeOffset.UtcNow.Add(ReservationTime).ToUnixTimeSeconds();
                    user.Name = twitterUser.Name;
                    user.Username = twitterUser.ScreenName;
                    user.IsProtected = twitterUser.Protected;
                    user.FollowersCount = twitterUser.FollowersCount;
                }
                catch (TwitterException e)
                {
                    switch (e.StatusCode)
                    {
                        case 403:
                            user.IsSuspended = true;
                            break;
                        case 404:
                            user.IsDeleted = true;
                            break;
                        default:
                            throw;
                    }
                }

                await context.SaveChangesAsync();

                if (user.IsProtected || user.IsSuspended || user.IsDeleted)
                {
                    Log.Information("Skipping user @{username} because their is either protected, suspended or deleted", user.Username);
                    return;
                }

                produced.Id = user.Id;
                produced.Username = user.Username;

                var existingTweets = user.Tweets.Any() ? user.Tweets.Select(t => t.Id).ToImmutableHashSet() : null;
                produced.Timeline = await TimelineCollection.CreateAsync(_twitterClient, _tweetRetriever, twitterUser, existingTweets);

                if (!await BeforeProduceAsync(context, user, produced))
                    continue;

                await ProduceAsync(produced);
            }
        }

        protected abstract Task<List<User>> GetUsersAsync(TwitterContext context);

        protected abstract Task<bool> BeforeProduceAsync(TwitterContext context, User user, T produced);
    }
}
