using Humanizer;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public static class ContentExtensions
    {
        private const int BatchSize = 50;
        private static readonly TimeSpan TimePerImage = 15.Seconds();
        private static readonly TimeSpan ReservationTime = TimePerImage * BatchSize;

        private static readonly AsyncLock Mutex = new();

        public static async Task<IEnumerable<TUnprocessedContentModel>> GetUnprocessedAsync<TContent, TUnprocessedContentModel>(
            this FluffleContext context, Func<FluffleContext, DbSet<TContent>> selectSet, Platform platform,
            Func<IQueryable<TContent>, IQueryable<TContent>> buildQuery = null, Action<TContent, TUnprocessedContentModel> mapModel = null)
            where TContent : Content
            where TUnprocessedContentModel : UnprocessedContentModel, new()
        {
            using var _ = await Mutex.LockAsync();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var contentQuery = selectSet(context).AsSplitQuery()
                .NotDeleted()
                .Include(c => c.Platform)
                .Include(c => c.Files)
                .Where(c => c.ReservedUntil < now)
                .Where(c => c.PlatformId == platform.Id && c.MediaTypeId == (int)MediaTypeConstant.Image)
                .Where(c => c.RequiresIndexing)
                .OrderBy(c => c.IsIndexed) // This puts images without hashes first
                .ThenByDescending(c => c.Priority)
                .ThenByDescending(c => c.CreatedAt)
                .Take(BatchSize);

            if (buildQuery != null)
                contentQuery = buildQuery(contentQuery);

            var unprocessedImages = await contentQuery.ToListAsync();

            foreach (var unprocessedImage in unprocessedImages)
                unprocessedImage.ReservedUntil = DateTimeOffset.UtcNow.Add(ReservationTime).ToUnixTimeSeconds();

            var models = unprocessedImages.Select(c =>
            {
                var model = new TUnprocessedContentModel
                {
                    ContentId = c.Id,
                    Platform = (PlatformConstant)platform.Id,
                    PlatformName = platform.Name,
                    IdOnPlatform = c.IdOnPlatform,
                    Files = c.Files.Select(sc => new UnprocessedContentModel.FileModel
                    {
                        Width = sc.Width,
                        Height = sc.Height,
                        Location = sc.Location,
                    })
                };
                mapModel?.Invoke(c, model);

                return model;
            });

            await context.SaveChangesAsync();
            return models;
        }

        public static async Task<SE> GetContentAsync<TContent>(this IQueryable<TContent> content,
            IQueryable<Platform> platforms, string platformName, string platformContentId,
            Func<TContent, Task<SE>> func, Func<SE> onNotFound = null) where TContent : Content
        {
            return await platforms.GetPlatformAsync(platformName, async platform =>
            {
                var cont = await content.FirstOrDefaultAsync(platform.Id, platformContentId);

                if (cont == null)
                {
                    return onNotFound == null
                        ? ContentError.ContentNotFound(platform.Name, platformContentId)
                        : onNotFound();
                }

                return await func(cont);
            });
        }
    }
}
