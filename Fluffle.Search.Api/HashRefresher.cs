using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Database.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api
{
    public class HashRefresher : IService, IInitializable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<HashRefresher> _logger;
        private readonly PlatformSearchService _compareService;

        private long _afterChangeId;

        public HashRefresher(IServiceProvider services, ILogger<HashRefresher> logger, PlatformSearchService compareService)
        {
            _services = services;
            _logger = logger;
            _compareService = compareService;
        }

        public Task InitializeAsync() => RefreshAsync(true);

        public Task RunAsync() => RefreshAsync(false);

        private async Task RefreshAsync(bool isFirstRun)
        {
            _logger.LogInformation("Refreshing hashes...");

            var stopwatch = Stopwatch.StartNew();

            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<FluffleSearchContext>();

            var images = context.Images.AsNoTracking()
                .Include(i => i.ImageHash)
                .Where(i => i.ChangeId > _afterChangeId)
                .OrderBy(i => i.ChangeId)
                .Select(i => new
                {
                    i.Id,
                    i.PlatformId,
                    i.ImageHash,
                    i.IsSfw,
                    i.ChangeId,
                    i.IsDeleted
                });

            foreach (var image in images)
            {
                // We can skip deleted images on the first run because they comparison service will be uninitialized
                if (image.IsDeleted && !isFirstRun)
                {
                    _compareService.Remove((PlatformConstant)image.PlatformId, image.Id);
                    continue;
                }

                var hash = FluffleHash.ToUInt64(image.ImageHash.PhashAverage64);
                _compareService.Add((PlatformConstant)image.PlatformId, new HashedImage(image.Id, hash), image.IsSfw);

                if (image.ChangeId > _afterChangeId)
                    _afterChangeId = image.ChangeId;
            }

            _logger.LogInformation("Hashes refreshed in {elapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
