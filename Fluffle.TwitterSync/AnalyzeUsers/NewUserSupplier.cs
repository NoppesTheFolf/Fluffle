using Humanizer;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class NewUserSupplier : BaseUserSupplier<AnalyzeUserData>
    {
        private const int UsersBatchSize = 20;
        private const int ImagesBatchSize = 20;
        private static readonly TimeSpan RetryTime = 2.Weeks();

        protected override TimeSpan Interval => 5.Minutes();
        protected override TimeSpan ReservationTime => 1.Hours();

        public NewUserSupplier(IServiceProvider services, ITwitterClient twitterClient,
            TweetRetriever tweetRetriever) : base(services, twitterClient, tweetRetriever)
        {
        }

        protected override Task<List<User>> GetUsersAsync(TwitterContext context)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return context.Users
                .Where(u => u.ReservedUntil < now && u.IsFurryArtist == null && u.IsOnE621 && !u.IsProtected && !u.IsSuspended && !u.IsDeleted)
                .OrderByDescending(u => u.FollowersCount)
                .Take(UsersBatchSize)
                .ToListAsync();
        }

        protected override async Task<bool> BeforeProduceAsync(TwitterContext context, User user, AnalyzeUserData produced)
        {
            produced.Images = produced.Timeline
                .Where(t => t.Type() == TweetType.Post && t.CreatedBy.IdStr == user.Id)
                .SelectMany(t => t.Media.Where(m => m.MediaType() == MediaTypeConstant.Image).Select(m => (t, m)))
                .OrderByDescending(x => x.t.FavoriteCount)
                .Take(ImagesBatchSize)
                .Select(x => new RetrieverImage
                {
                    TweetId = x.t.IdStr,
                    MediaId = x.m.IdStr,
                    Url = x.m.MediaURLHttps,
                    Sizes = x.m.Sizes.Select(s => new RetrieverSize
                    {
                        Width = (int)s.Value.Width,
                        Height = (int)s.Value.Height,
                        Resize = s.Value.Resize(),
                        Size = s.Size()
                    }).ToList()
                })
                .ToList();

            if (produced.Images.Count >= ImagesBatchSize)
                return true;

            Log.Information("Skipping user @{username} for now because they have not uploaded enough images yet", user.Username);
            user.ReservedUntil = DateTimeOffset.UtcNow.Add(RetryTime).ToUnixTimeSeconds();
            await context.SaveChangesAsync();

            return false;
        }
    }
}
