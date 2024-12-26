using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Image = Noppes.Fluffle.Search.Database.Models.Image;
using Platform = Noppes.Fluffle.Search.Database.Models.Platform;

namespace Noppes.Fluffle.Search.Api;

public class SyncService : IService
{
    private static readonly TimeSpan NextSyncDelay = TimeSpan.FromSeconds(3);

    private readonly ILogger<SyncService> _logger;
    private readonly FluffleClient _client;
    private readonly IServiceProvider _serviceProvider;

    public SyncService(ILogger<SyncService> logger, FluffleClient client, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _client = client;
        _serviceProvider = serviceProvider;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Synchronizing platforms...");
        var platforms = await RefreshPlatformsAsync();

        var tasks = new List<Task>();
        foreach (var platform in platforms)
        {
            var task = Task.Run(async () =>
            {
                _logger.LogInformation("Synchronizing for {platform}...", platform.Name);

                await RefreshCreditableEntitiesAsync(platform);
                await RefreshImagesAsync(platform);
            });

            tasks.Add(task);
            await Task.WhenAny(task, Task.Delay(NextSyncDelay));
        }

        do
        {
            var task = await Task.WhenAny(tasks);
            tasks.Remove(task);

            if (task.Exception == null)
                continue;

            _logger.LogError(task.Exception, "Synchronization process crashed.");
        } while (tasks.Any());
    }

    public async Task<IList<PlatformModel>> RefreshPlatformsAsync()
    {
        var platforms = await HttpResiliency.RunAsync(() => _client.GetPlatformsAsync(), onRetry: _ =>
        {
            _logger.LogWarning("A transient error has occurred while trying to sync platforms.");
        });

        await UseContextResilientAsync(async context =>
        {
            foreach (var platform in platforms)
            {
                var dbPlatform = await context.Platforms.FindAsync(platform.Id);

                if (dbPlatform != null)
                {
                    dbPlatform.Name = platform.Name;
                    dbPlatform.NormalizedName = platform.NormalizedName;
                    continue;
                }

                await context.Platforms.AddAsync(new Platform
                {
                    Id = platform.Id,
                    Name = platform.Name,
                    NormalizedName = platform.NormalizedName
                });
            }

            await context.SaveChangesAsync();
        });

        return platforms;
    }

    public async Task RefreshCreditableEntitiesAsync(PlatformModel platform)
    {
        await RefreshAsync<CreditableEntity, CreditableEntitiesSyncModel, CreditableEntitiesSyncModel.CreditableEntityModel>(
            platform.Id, c => c.CreditableEntities, afterChangeId =>
            {
                _logger.LogInformation("Retrieving creditable entities after change ID {changeId} for platform {platform}...", afterChangeId, platform.Name);
                return _client.GetSyncCreditableEntitiesAsync(platform.NormalizedName, afterChangeId);
            },
            async (context, models) =>
            {
                var creditableEntities = models.Select(x => x.MapTo<CreditableEntity>()).ToList();

                var existingCreditableEntities = await context.CreditableEntities
                    .Where(x => creditableEntities.Select(y => y.Id).Contains(x.Id))
                    .ToListAsync();

                await context.SynchronizeAsync(c => c.CreditableEntities, existingCreditableEntities, creditableEntities,
                    (ce1, ce2) => ce1.Id == ce2.Id, updateAnywayAsync: (src, dest) =>
                    {
                        dest.PlatformId = src.PlatformId;
                        dest.Name = src.Name;
                        dest.Type = src.Type;
                        dest.Priority = src.Priority;
                        dest.ChangeId = src.ChangeId;

                        return Task.CompletedTask;
                    });
            });
    }

    public async Task RefreshImagesAsync(PlatformModel platform)
    {
        await RefreshAsync<Image, ImagesSyncModel, ImagesSyncModel.ImageModel>(platform.Id, c => c.Images, afterChangeId =>
            {
                _logger.LogInformation("Retrieving images after change ID {changeId} for platform {platform}...", afterChangeId, platform.Name);

                return _client.GetSyncImagesAsync(platform.NormalizedName, afterChangeId);
            },
            async (context, models) =>
            {
                var modelLookup = models.ToDictionary(m => m.Id);

                var existingImages = await context.Images
                    .Where(i => modelLookup.Values.Select(m => m.Id).Contains(i.Id))
                    .ToListAsync();
                var existingImageIds = existingImages.Select(edi => edi.Id).ToHashSet();

                var newImages = modelLookup.Values.Select(m =>
                {
                    byte[] compressedImageHashes = null;
                    if (m.Hash != null)
                    {
                        var imageHashes = m.Hash.PhashAverage64
                            .Concat(m.Hash.PhashRed256).Concat(m.Hash.PhashGreen256).Concat(m.Hash.PhashBlue256).Concat(m.Hash.PhashAverage256)
                            .Concat(m.Hash.PhashRed1024).Concat(m.Hash.PhashGreen1024).Concat(m.Hash.PhashBlue1024).Concat(m.Hash.PhashAverage1024)
                            .ToArray();

                        using var compressedStream = new MemoryStream();
                        using (var brotliStream = new BrotliStream(compressedStream, CompressionLevel.SmallestSize))
                        {
                            brotliStream.Write(imageHashes);
                        }

                        compressedImageHashes = compressedStream.ToArray();
                    }

                    return new Image
                    {
                        Id = m.Id,
                        PlatformId = m.PlatformId,
                        Location = m.ViewLocation,
                        IsSfw = m.IsSfw,
                        CompressedImageHashes = compressedImageHashes,
                        ThumbnailLocation = m.Thumbnail?.Location,
                        ThumbnailWidth = m.Thumbnail?.Width ?? -1,
                        ThumbnailCenterX = m.Thumbnail?.CenterX ?? -1,
                        ThumbnailHeight = m.Thumbnail?.Height ?? -1,
                        ThumbnailCenterY = m.Thumbnail?.CenterY ?? -1,
                        Credits = m.Credits?.ToArray(),
                        ChangeId = m.ChangeId,
                        IsDeleted = m.IsDeleted
                    };
                }).ToList();

                // Skip the deleted images which are also not in the database
                newImages = newImages
                    .Where(x => !x.IsDeleted || (x.IsDeleted && existingImageIds.Contains(x.Id)))
                    .ToList();

                await context.SynchronizeAsync(x => x.Images, existingImages,
                    newImages, (x1, x2) => x1.Id == x2.Id, onUpdateAsync: (src, dest) =>
                    {
                        dest.PlatformId = src.PlatformId;
                        dest.Location = src.Location;
                        dest.IsSfw = src.IsSfw;

                        if (src.CompressedImageHashes != null)
                        {
                            dest.CompressedImageHashes = src.CompressedImageHashes;
                        }

                        if (src.ThumbnailLocation != null)
                        {
                            dest.ThumbnailLocation = src.ThumbnailLocation;
                            dest.ThumbnailWidth = src.ThumbnailWidth;
                            dest.ThumbnailCenterX = src.ThumbnailCenterX;
                            dest.ThumbnailHeight = src.ThumbnailHeight;
                            dest.ThumbnailCenterY = src.ThumbnailCenterY;
                        }

                        if (src.Credits != null)
                        {
                            dest.Credits = src.Credits;
                        }

                        dest.ChangeId = src.ChangeId;
                        dest.IsDeleted = src.IsDeleted;

                        return Task.CompletedTask;
                    });

                // First we check if we have all the credits defined in the model
                var creditsInModels = modelLookup.Values
                    .Where(m => !m.IsDeleted) // Deleted content doesn't contain credits
                    .SelectMany(m => m.Credits)
                    .Distinct()
                    .ToList();

                var numberOfExistingCredits = await context.CreditableEntities
                    .CountAsync(ce => creditsInModels.Contains(ce.Id));

                // We're missing credits! So we're going to sync those first
                if (creditsInModels.Count != numberOfExistingCredits)
                    await RefreshCreditableEntitiesAsync(platform);
            });
    }

    public async Task RefreshAsync<TEntity, TModel, TModelData>(int platformId, Func<FluffleSearchContext, DbSet<TEntity>> getSet,
        Func<long, Task<TModel>> getModel, Func<FluffleSearchContext, IEnumerable<TModelData>, Task> processAsync)
        where TEntity : class, ITrackable where TModel : ITrackableModel<TModelData>
    {
        long afterChangeId = 0;
        await UseContextResilientAsync(async context =>
        {
            var baseQuery = getSet(context).Where(i => i.PlatformId == platformId);

            afterChangeId = await baseQuery
                .OrderByDescending(i => i.ChangeId)
                .Select(i => i.ChangeId)
                .FirstOrDefaultAsync();
        });

        while (true)
        {
            var model = await HttpResiliency.RunAsync(() => getModel(afterChangeId), onRetry: _ =>
            {
                _logger.LogWarning("A transient error has occurred while trying to sync.");
            });

            if (!model.Results.Any())
                break;

            await UseContextResilientAsync(async context =>
            {
                await processAsync(context, model.Results);

                await context.SaveChangesAsync();
            });

            afterChangeId = model.NextChangeId;
        }
    }

    public async Task UseContextResilientAsync(Func<FluffleSearchContext, Task> useContextAsync)
    {
        using var scope = _serviceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<FluffleSearchContext>();

        await context.ResilientAsync(async resilientContext =>
        {
            await useContextAsync(resilientContext);

            return true;
        }, () =>
        {
            _logger.LogWarning("A transient exception occurred while trying to work with the database.");
        });
    }
}
