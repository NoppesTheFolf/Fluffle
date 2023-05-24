using Humanizer;
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
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api;

public class SyncService : IService
{
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
            await Task.Delay(3.Seconds());
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
                var dbPlatform = await context.Platform.FindAsync(platform.Id);

                if (dbPlatform != null)
                {
                    dbPlatform.Name = platform.Name;
                    dbPlatform.NormalizedName = platform.NormalizedName;
                    continue;
                }

                await context.Platform.AddAsync(new Platform
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
                var creditableEntities = models.Select(m => m.MapTo<CreditableEntity>()).ToList();

                var existingCreditableEntities = await context.CreditableEntities
                    .Where(ce => creditableEntities.Select(ce => ce.Id).Contains(ce.Id))
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

                var existingDenormalizedImages = await context.DenormalizedImages
                    .Where(i => modelLookup.Values.Select(m => m.Id).Contains(i.Id))
                    .ToListAsync();
                var existingDenormalizedImageIds = existingDenormalizedImages.Select(edi => edi.Id).ToHashSet();

                var newDenormalizedImages = modelLookup.Values.Select(m => new DenormalizedImage
                {
                    Id = m.Id,
                    PlatformId = m.PlatformId,
                    Location = m.ViewLocation,
                    IsSfw = m.IsSfw,
                    PhashAverage64 = m.Hash?.PhashAverage64,
                    PhashRed256 = m.Hash?.PhashRed256,
                    PhashGreen256 = m.Hash?.PhashGreen256,
                    PhashBlue256 = m.Hash?.PhashBlue256,
                    PhashAverage256 = m.Hash?.PhashAverage256,
                    PhashRed1024 = m.Hash?.PhashRed1024,
                    PhashGreen1024 = m.Hash?.PhashGreen1024,
                    PhashBlue1024 = m.Hash?.PhashBlue1024,
                    PhashAverage1024 = m.Hash?.PhashAverage1024,
                    ThumbnailLocation = m.Thumbnail?.Location,
                    ThumbnailWidth = m.Thumbnail?.Width ?? -1,
                    ThumbnailCenterX = m.Thumbnail?.CenterX ?? -1,
                    ThumbnailHeight = m.Thumbnail?.Height ?? -1,
                    ThumbnailCenterY = m.Thumbnail?.CenterY ?? -1,
                    Credits = m.Credits?.ToArray(),
                    ChangeId = m.ChangeId,
                    IsDeleted = m.IsDeleted
                }).Where(ndi => !ndi.IsDeleted || existingDenormalizedImageIds.Contains(ndi.Id)).ToList();

                var result = await context.SynchronizeAsync(c => c.DenormalizedImages, existingDenormalizedImages,
                    newDenormalizedImages, (i1, i2) => i1.Id == i2.Id, onUpdateAsync: (src, dest) =>
                    {
                        dest.PlatformId = src.PlatformId;
                        dest.Location = src.Location;
                        dest.IsSfw = src.IsSfw;

                        if (src.PhashAverage64 != null)
                        {
                            dest.PhashAverage64 = src.PhashAverage64;

                            dest.PhashRed256 = src.PhashRed256;
                            dest.PhashGreen256 = src.PhashGreen256;
                            dest.PhashBlue256 = src.PhashBlue256;
                            dest.PhashAverage256 = src.PhashAverage256;

                            dest.PhashRed1024 = src.PhashRed1024;
                            dest.PhashGreen1024 = src.PhashGreen1024;
                            dest.PhashBlue1024 = src.PhashBlue1024;
                            dest.PhashAverage1024 = src.PhashAverage1024;
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

                // Then we sync the base content entities
                var imagesInModel = modelLookup.Values
                    .Select(m => m.MapTo<Image>())
                    .ToList();

                var existingImages = await context.Images
                    .IncludeThumbnails()
                    .Include(i => i.ContentCreditableEntities)
                    .Include(i => i.Files)
                    .Include(i => i.ImageHash)
                    .Where(i => imagesInModel.Select(m => m.Id).Contains(i.Id))
                    .ToListAsync();

                // Skip the deleted content pieces which are also not in the database
                var existingImageIds = existingImages.Select(i => i.Id).ToHashSet();
                imagesInModel = imagesInModel
                    .Except(imagesInModel.Where(m => m.IsDeleted && !existingImageIds.Contains(m.Id)))
                    .ToList();

                // Syncronize the images themselves (so not the hash etc)
                var imageSyncResult = await context.SynchronizeAsync(c => c.Images, existingImages, imagesInModel, (i1, i2) =>
                {
                    return i1.Id == i2.Id;
                }, onUpdateAsync: (src, dest) =>
                {
                    dest.IdOnPlatform = src.IdOnPlatform;
                    dest.PlatformId = src.PlatformId;
                    dest.IsDeleted = src.IsDeleted;
                    dest.ViewLocation = src.ViewLocation;
                    dest.IsSfw = src.IsSfw;

                    return Task.CompletedTask;
                }, updateAnywayAsync: (src, dest) =>
                {
                    dest.ChangeId = src.ChangeId;

                    return Task.CompletedTask;
                });
                var syncedImages = imageSyncResult.Results().Select(r => r.Entity).ToList();

                // Synchronize the images their attribute
                foreach (var image in syncedImages.Where(i => !i.IsDeleted))
                {
                    var model = modelLookup[image.Id];

                    // Synchronize the image its hash
                    if (image.ImageHash == null)
                        await context.ImageHashes.AddAsync(model.MapTo<ImageHash>());
                    else
                        model.MapTo(image.ImageHash);

                    // Synchronize the image its thumbnail
                    if (image.Thumbnail == null)
                    {
                        image.Thumbnail = model.Thumbnail.MapTo<Database.Models.Thumbnail>();
                        await context.Thumbnails.AddAsync(image.Thumbnail);
                    }
                    else
                    {
                        model.Thumbnail.MapTo(image.Thumbnail);
                    }

                    // Synchronize the image its credits
                    var modelCredits = model.Credits.Select(cei => new ContentCreditableEntity
                    {
                        ContentId = image.Id,
                        CreditableEntityId = cei
                    }).ToList();

                    await context.SynchronizeAsync(c => c.ContentCreditableEntities, image.ContentCreditableEntities, modelCredits,
                        (ce1, ce2) =>
                        {
                            return (ce1.ContentId, ce1.CreditableEntityId) == (ce2.ContentId, ce2.CreditableEntityId);
                        });

                    // Synchronize the image its files
                    var modelFiles = model.Files.Select(f => new ContentFile
                    {
                        ContentId = model.Id,
                        Location = f.Location,
                        Format = f.Format,
                        Width = f.Width,
                        Height = f.Height
                    }).ToList();

                    await context.SynchronizeAsync(c => c.ContentFiles, image.Files, modelFiles, (c1, c2) =>
                    {
                        return (c1.ContentId, c1.Location) == (c2.ContentId, c2.Location);
                    }, onUpdateAsync: (src, dest) =>
                    {
                        dest.Format = src.Format;
                        dest.Width = src.Width;
                        dest.Height = src.Height;

                        return Task.CompletedTask;
                    });
                }
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
