using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi.Models.V2;
using static MoreLinq.Extensions.BatchExtension;
using User = Noppes.Fluffle.TwitterSync.Database.Models.User;

namespace Noppes.Fluffle.TwitterSync
{
    public partial class SyncClient
    {
        /// <summary>
        /// Matches URLs that contain a semantically valid Twitter handle.
        /// </summary>
        private static readonly Regex TwitterRegex = new("twitter\\.com\\/([A-Za-z0-9_]{1,15})(?=\\/|$|\\?)", RegexOptions.Compiled);

        private async Task<int> CalculateStartIdAsync()
        {
            using var scope = Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            if (!await context.E621Artists.AnyAsync())
                return 0;

            return await context.E621Artists.MaxAsync(a => a.Id);
        }

        private async Task SyncE621ArtistsAsync(CancellationToken cancellationToken)
        {
            var startId = await CalculateStartIdAsync();
            await foreach (var (artists, _, _) in EnumerateArtistsAsync(startId).WithCancellation(cancellationToken))
            {
                using var scope = Services.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

                var artistIds = artists.Select(a => a.Id);
                var existingArtists = await context.E621Artists
                    .Include(a => a.Urls)
                    .Where(a => artistIds.Contains(a.Id))
                    .ToListAsync(cancellationToken);

                var newArtists = artists.Select(a => new E621Artist
                {
                    Id = a.Id,
                    Name = a.Name.RemoveNullChar()
                }).ToList();

                var artistSyncResult = await context.SynchronizeAsync(c => c.E621Artists, existingArtists, newArtists,
                    (a1, a2) => a1.Id == a2.Id, onUpdateAsync: (src, dest) =>
                    {
                        dest.Name = src.Name;

                        return Task.CompletedTask;
                    });
                artistSyncResult.Print();

                var existingUrls = existingArtists
                    .SelectMany(a => a.Urls)
                    .ToList();

                var newUrls = artists
                    .SelectMany(a => a.Urls)
                    .Where(u => u.Location != null && u.IsActive)
                    .Select(u => (url: u, match: TwitterRegex.Match(u.Location.OriginalString.Trim())))
                    .Where(x => x.match.Success)
                    .Select(x => new E621ArtistUrl
                    {
                        Id = x.url.Id,
                        ArtistId = x.url.ArtistId,
                        TwitterUsername = x.match.Groups[1].Value
                    }).ToList();

                var urlSyncResult = await context.SynchronizeAsync(c => c.E621ArtistUrls, existingUrls, newUrls,
                    (ae1, ae2) => ae1.Id == ae2.Id, onUpdateAsync: (src, dest) =>
                    {
                        dest.ArtistId = src.ArtistId;
                        dest.TwitterUsername = src.TwitterUsername;

                        return Task.CompletedTask;
                    }, onUpdateChangesAsync: (_, dest) =>
                    {
                        dest.TwitterExists = null;

                        return Task.CompletedTask;
                    });
                urlSyncResult.Print();

                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private async IAsyncEnumerable<(ICollection<Artist> artists, int afterId, int maxId)> EnumerateArtistsAsync(int startId)
        {
            var currentId = startId;
            while (true)
            {
                var artists = await LogEx.TimeAsync(async () =>
                {
                    return await HttpResiliency.RunAsync(() =>
                        _e621Client.GetArtistsAsync(currentId, Position.After, limit: E621Constants.ArtistsMaximumLimit));
                }, "Retrieving artists after ID {afterId}", currentId);

                if (!artists.Any())
                {
                    Log.Information("No artists were retrieved");
                    break;
                }

                var maxId = artists.Max(p => p.Id);
                yield return (artists, currentId, maxId);

                if (artists.Count != E621Constants.PostsMaximumLimit)
                    break;

                currentId = maxId;
            }
        }

        private async Task SyncTwitterArtists(CancellationToken cancellationToken)
        {
            using var scope = Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            var artistsToSync = await context.E621ArtistUrls
                .Where(au => au.TwitterExists == null)
                .ToListAsync(cancellationToken);

            foreach (var batch in artistsToSync.Batch(100).Select(b => b.ToList()))
            {
                var usernames = batch.Select(a => a.TwitterUsername.ToLowerInvariant()).Distinct().ToArray();
                var response = await HttpResiliency.RunAsync(() => _twitterClient.UsersV2.GetUsersByNameAsync(usernames));
                var users = response.Users ?? Array.Empty<UserV2>();
                var responseLookup = users.ToDictionary(u => u.Username, u => u, StringComparer.InvariantCultureIgnoreCase);

                var newUsers = users.Select(u => new User
                {
                    Id = u.Id,
                    Name = u.Name,
                    Username = u.Username,
                    IsProtected = u.IsProtected,
                    FollowersCount = u.PublicMetrics.FollowersCount,
                    IsOnE621 = true
                }).ToList();

                var existingUsers = await context.Users
                    .Where(u => newUsers.Select(nu => nu.Id).Contains(u.Id))
                    .ToListAsync(cancellationToken);

                var syncResult = await context.SynchronizeAsync(c => c.Users, existingUsers, newUsers,
                    (tu1, tu2) => tu1.Id == tu2.Id, onUpdateAsync: (src, dest) =>
                    {
                        dest.Name = src.Name;
                        dest.Username = src.Username;
                        dest.IsProtected = src.IsProtected;
                        dest.FollowersCount = src.FollowersCount;
                        dest.IsOnE621 = src.IsOnE621;

                        return Task.CompletedTask;
                    });
                syncResult.Print();

                foreach (var url in batch)
                    url.TwitterExists = responseLookup.ContainsKey(url.TwitterUsername);

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
