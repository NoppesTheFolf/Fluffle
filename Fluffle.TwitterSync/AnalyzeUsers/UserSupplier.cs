using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class UserSupplier : Producer<AnalyzeUserData>
    {
        private const int BatchSize = 20;
        private static readonly TimeSpan Interval = 1.Minutes();
        private static readonly TimeSpan ReservationTime = 1.Hours();
        private static readonly TimeSpan RetryTime = 2.Weeks();

        private readonly IServiceProvider _services;
        private readonly ITwitterClient _twitterClient;

        public UserSupplier(IServiceProvider services, ITwitterClient twitterClient)
        {
            _services = services;
            _twitterClient = twitterClient;
        }

        public override async Task WorkAsync()
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var users = await context.Users
                .Where(u => u.ReservedUntil < now && u.IsFurryArtist == null && u.IsOnE621 && !u.IsProtected && !u.IsSuspended)
                .OrderByDescending(u => u.FollowersCount)
                .Take(20)
                .ToListAsync();

            if (users.Count == 0)
            {
                Log.Information("Waiting for {interval} before trying to supply users again", Interval);
                await Task.Delay(Interval);
                return;
            }

            IUser twitterUser = null;
            foreach (var user in users)
            {
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
                    if (e.StatusCode == 403)
                        user.IsSuspended = true;
                    else
                        throw;
                }

                await context.SaveChangesAsync();

                if (user.IsProtected || user.IsSuspended)
                {
                    Log.Information("Skipping user @{username} because their is either protected or suspended", user.Username);
                    return;
                }

                var timeline = await TimelineCollection.CreateAsync(_twitterClient, twitterUser);

                var images = timeline
                    .Where(t => t.Type() == TweetType.Post && t.CreatedBy.IdStr == user.Id)
                    .SelectMany(t => t.Media.Where(m => m.MediaType() == MediaTypeConstant.Image).Select(m => (t, m)))
                    .OrderByDescending(x => x.t.FavoriteCount)
                    .Take(BatchSize)
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

                if (images.Count < BatchSize)
                {
                    Log.Information("Skipping user @{username} for now because they have not uploaded enough images yet", user.Username);
                    user.ReservedUntil = DateTimeOffset.UtcNow.Add(RetryTime).ToUnixTimeSeconds();
                    await context.SaveChangesAsync();

                    return;
                }

                await ProduceAsync(new AnalyzeUserData
                {
                    Id = user.Id,
                    Username = user.Username,
                    Timeline = timeline,
                    Images = images
                });
            }
        }
    }
}
