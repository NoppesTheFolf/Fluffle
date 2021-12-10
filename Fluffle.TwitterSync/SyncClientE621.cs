using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Communication;
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
        private static readonly Regex TwitterUsernameRegex = new("twitter\\.com\\/([A-Za-z0-9_]{1,15})(?=\\/|$|\\?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Matches URLs that contain a semantically valid Twitter tweet IDs.
        /// </summary>
        private static readonly Regex TwitterStatusRegex = new("twitter\\.com\\/.*\\/status\\/([0-9]*)(?=\\/|$|\\?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private async Task SynchronizeAsync(int afterId)
        {
            while (true)
            {
                using var scope = Services.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

                Log.Information("Synchronizing other sources after ID {afterId}", afterId);
                var sources = await HttpResiliency.RunAsync(() => _fluffleClient.GetOtherSourcesAsync(afterId));

                await context.OtherSources.AddRangeAsync(sources.Select(s => new OtherSource
                {
                    Id = s.Id,
                    Location = s.Location
                }));
                await context.SaveChangesAsync();

                if (sources.Count != Endpoints.SourcesLimit)
                    break;

                afterId = sources.Max(s => s.Id);
            }
        }

        private async Task ExtractAsync()
        {
            while (true)
            {
                using var scope = Services.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

                var sources = await context.OtherSources
                    .Where(os => !os.HasBeenProcessed)
                    .OrderByDescending(os => os.Id)
                    .Take(Endpoints.SourcesLimit)
                    .ToListAsync();

                var statusIds = new List<long>();
                var usernames = new List<string>();
                foreach (var source in sources)
                {
                    source.HasBeenProcessed = true;

                    var statusMatch = TwitterStatusRegex.Match(source.Location);
                    var statusIdStr = statusMatch.Groups[1].Value.Trim();
                    if (statusMatch.Success && long.TryParse(statusIdStr, out var statusId))
                    {
                        statusIds.Add(statusId);
                        continue;
                    }

                    var usernameMatch = TwitterUsernameRegex.Match(source.Location);
                    if (!usernameMatch.Success)
                        continue;

                    usernames.Add(usernameMatch.Groups[1].Value);
                }

                await ProcessUsers(context, usernames, statusIds);
                await context.SaveChangesAsync();

                if (sources.Count != Endpoints.SourcesLimit)
                    break;
            }
        }

        public async Task SyncOtherSourcesAsync()
        {
            using var scope = Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            var afterId = await context.OtherSources
                .OrderByDescending(os => os.Id)
                .Select(os => os.Id)
                .FirstOrDefaultAsync();

            await SynchronizeAsync(afterId);
            await ExtractAsync();
        }

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
                    .Select(u => (url: u, match: TwitterUsernameRegex.Match(u.Location.OriginalString.Trim())))
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
                    return await E621HttpResiliency.RunAsync(() =>
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
                var users = await ProcessUsers(context, batch.Select(a => a.TwitterUsername));
                var usersLookup = users.Select(u => u.Username).ToHashSet(StringComparer.InvariantCultureIgnoreCase);

                foreach (var url in batch)
                    url.TwitterExists = usersLookup.Contains(url.TwitterUsername);

                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task<ICollection<UserV2>> ProcessUsers(TwitterContext context, IEnumerable<string> usernames = null, IEnumerable<long> tweetIds = null)
        {
            var users = new Dictionary<string, UserV2>(StringComparer.InvariantCultureIgnoreCase);

            void ProcessResponse(UsersV2Response response)
            {
                if (response.Users == null)
                    return;

                foreach (var user in response.Users)
                    users[user.Username] = user;
            }

            if (usernames != null)
            {
                foreach (var usernameBatch in usernames.Select(u => u.ToLowerInvariant()).Distinct().Batch(100))
                {
                    var response = await HttpResiliency.RunAsync(() => _twitterClient.UsersV2.GetUsersByNameAsync(usernameBatch.ToArray()));
                    ProcessResponse(response);
                }
            }

            if (tweetIds != null)
            {
                var priority = await _tweetRetriever.AcquirePriorityAsync();
                var tweets = await _tweetRetriever.GetTweets(priority, tweetIds.Distinct().ToList());
                foreach (var tweetBatch in tweets.Batch(100))
                {
                    var userIds = tweetBatch.Select(t => t.CreatedBy.IdStr).ToArray();

                    var response = await HttpResiliency.RunAsync(() => _twitterClient.UsersV2.GetUsersByIdAsync(userIds));
                    ProcessResponse(response);
                }
            }

            var newUsers = users.Values.Select(u => new User
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
                .ToListAsync();

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

            return users.Values;
        }
    }
}
