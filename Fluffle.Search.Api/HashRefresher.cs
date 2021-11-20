using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Api.Filters;
using Noppes.Fluffle.Search.Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api
{
    public class HashRefresher : IService, IInitializable
    {
        private const int BatchSize = 20_000;

        private readonly IServiceProvider _services;
        private readonly ILogger<HashRefresher> _logger;
        private readonly PlatformSearchService _compareService;

        private IDictionary<int, long> _afterChangeIds;

        public HashRefresher(IServiceProvider services, ILogger<HashRefresher> logger, PlatformSearchService compareService)
        {
            _services = services;
            _logger = logger;
            _compareService = compareService;
            _afterChangeIds = new Dictionary<int, long>();
        }

        public Task InitializeAsync() => RefreshAsync(true);

        public Task RunAsync() => RefreshAsync(false);

        private async Task RefreshAsync(bool isFirstRun)
        {
            _logger.LogInformation("Refreshing hashes...");

            var stopwatch = Stopwatch.StartNew();

            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<FluffleSearchContext>();

            context.Database.SetCommandTimeout(5.Minutes());

            foreach (var platform in await context.Platform.ToListAsync())
            {
                while (true)
                {
                    _afterChangeIds.TryGetValue(platform.Id, out var afterChangeId);

                    var images = await context.Images.AsNoTracking()
                        .Include(i => i.ImageHash)
                        .Where(i => i.PlatformId == platform.Id)
                        .Where(i => i.ChangeId > afterChangeId)
                        .OrderBy(i => i.ChangeId)
                        .Take(BatchSize)
                        .Select(i => new
                        {
                            i.Id,
                            i.PlatformId,
                            i.ImageHash.PhashAverage64,
                            i.IsSfw,
                            i.ChangeId,
                            i.IsDeleted
                        }).ToListAsync();

                    foreach (var image in images)
                    {
                        // We can skip deleted images on the first run because the comparison service will be uninitialized
                        if (image.IsDeleted && !isFirstRun)
                        {
                            _compareService.Remove((PlatformConstant)image.PlatformId, image.Id);
                            continue;
                        }

                        var hash = FluffleHash.ToUInt64(image.PhashAverage64);
                        _compareService.Add((PlatformConstant)image.PlatformId, new HashedImage(image.Id, hash), image.IsSfw);

                        if (image.ChangeId > afterChangeId)
                            _afterChangeIds[platform.Id] = image.ChangeId;
                    }

                    if (images.Count < BatchSize)
                        break;
                }
            }

            StartupFilter.HasStarted = true;
            _logger.LogInformation("Hashes refreshed in {elapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
