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

namespace Noppes.Fluffle.Search.Api
{
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

            foreach (var platform in platforms)
            {
                _logger.LogInformation("Synchronizing for {platform}...", platform);
                await RefreshCreditableEntitiesAsync(platform);
                await RefreshImagesAsync(platform);
            }
        }

        public async Task<IEnumerable<PlatformModel>> RefreshPlatformsAsync()
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
                    _logger.LogInformation("Retrieving creditable entities after change ID {changeId}...", afterChangeId);
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
                            dest.ChangeId = src.ChangeId;

                            return Task.CompletedTask;
                        });
                });
        }

        public async Task RefreshImagesAsync(PlatformModel platform)
        {
            await RefreshAsync<Image, ImagesSyncModel, ImagesSyncModel.ImageModel>(platform.Id, c => c.Images, afterChangeId =>
                {
                    _logger.LogInformation("Retrieving images after change ID {changeId}...", afterChangeId);

                    return _client.GetSyncImagesAsync(platform.NormalizedName, afterChangeId);
                },
                async (context, models) =>
                {
                    var modelLookup = models.ToDictionary(m => m.Id);

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
                            image.Thumbnail = model.Thumbnail.MapTo<Thumbnail>();
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

                if (await baseQuery.AnyAsync())
                    afterChangeId = await baseQuery.MaxAsync(i => i.ChangeId);
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
}
