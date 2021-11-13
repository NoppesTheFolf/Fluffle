using Humanizer;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.TwitterSync.AnalyzeUsers;
using Noppes.Fluffle.TwitterSync.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;

namespace Noppes.Fluffle.TwitterSync.RefreshTimeline
{
    public class RefreshUserSupplier : BaseUserSupplier<RefreshTimelineData>
    {
        private const int BatchSize = 20;

        protected override TimeSpan Interval => 30.Minutes();
        protected override TimeSpan ReservationTime => 1.Hours();

        private readonly TwitterSyncConfiguration _syncConf;

        public RefreshUserSupplier(IServiceProvider services, ITwitterClient twitterClient, TwitterSyncConfiguration syncConf) : base(services, twitterClient)
        {
            _syncConf = syncConf;
        }

        protected override async Task<List<User>> GetUsersAsync(TwitterContext context)
        {
            var now = DateTimeOffset.UtcNow;
            var nowUnix = now.ToUnixTimeSeconds();

            var users = await context.Users
                .Include(u => u.Tweets)
                .Where(u => u.ReservedUntil < nowUnix && u.TimelineRetrievedAt != null && u.IsFurryArtist == true && !u.IsProtected && !u.IsSuspended && !u.IsDeleted)
                .OrderBy(u => u.TimelineRetrievedAt)
                .Take(BatchSize)
                .ToListAsync();

            users = users
                .Where(u => now.Subtract((DateTimeOffset)u.TimelineRetrievedAt) > _syncConf.TimelineExpirationInterval.Hours())
                .ToList();

            return users;
        }

        protected override Task<bool> BeforeProduceAsync(TwitterContext context, User user, RefreshTimelineData produced)
        {
            return Task.FromResult(true);
        }
    }
}
