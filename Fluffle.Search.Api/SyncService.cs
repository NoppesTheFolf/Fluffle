using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using System;
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
            _logger.LogInformation("Synchronizing with main...");

            await RefreshPlatformsAsync();
            await RefreshCreditableEntitiesAsync();
            await RefreshImagesAsync();
        }

        public async Task RefreshPlatformsAsync()
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
        }

        public async Task RefreshCreditableEntitiesAsync()
        {
            await RefreshAsync<CreditableEntity, CreditableEntitiesSyncModel, CreditableEntitiesSyncModel.CreditableEntityModel>(
                c => c.CreditableEntities, afterChangeId =>
                {
                    _logger.LogInformation("Retrieving creditable entities after change ID {changeId}...", afterChangeId);
                    return _client.GetSyncCreditableEntitiesAsync(afterChangeId);
                },
                async (context, model) =>
                {
                    var creditableEntity = await context.CreditableEntities
                        .FirstOrDefaultAsync(ce => ce.Id == model.Id);

                    if (creditableEntity == null)
                    {
                        await context.CreditableEntities.AddAsync(model.MapTo<CreditableEntity>());
                        return;
                    }

                    model.MapTo(creditableEntity);
                });
        }

        public async Task RefreshImagesAsync()
        {
            await RefreshAsync<Image, ImagesSyncModel, ImagesSyncModel.ImageModel>(c => c.Images, afterChangeId =>
                {
                    _logger.LogInformation("Retrieving images after change ID {changeId}...", afterChangeId);
                    return _client.GetSyncImagesAsync(afterChangeId);
                },
                async (context, model) =>
                {
                    var image = await context.Images
                        .IncludeThumbnails()
                        .Include(i => i.ContentCreditableEntities)
                        .Include(i => i.Files)
                        .Include(i => i.ImageHash)
                        .FirstOrDefaultAsync(i => i.Id == model.Id);

                    if (image == null)
                    {
                        // If the image is deleted, but also hasn't been synchronized before, we can
                        // simply ignore it
                        if (model.IsDeleted)
                            return;

                        await context.Images.AddAsync(model.MapTo<Image>());
                        return;
                    }

                    // Entity Framework really hates it if we replace a collection, so instead to
                    // just delete all the existing thumbnails etc so that we can then safely insert
                    // them again
                    context.Thumbnails.Remove(image.Thumbnail);
                    context.ContentCreditableEntities.RemoveRange(image.ContentCreditableEntities);
                    context.ContentFiles.RemoveRange(image.Files);
                    context.ImageHashes.Remove(image.ImageHash);

                    // The model doesn't contain credits if the content is deleted
                    if (!model.IsDeleted)
                    {
                        var numberOfExistingCredits = await context.CreditableEntities
                            .CountAsync(ce => model.Credits.Contains(ce.Id));

                        // We're missing credits! So we're going to sync those first
                        if (model.Credits.Count() != numberOfExistingCredits)
                            await RefreshCreditableEntitiesAsync();
                    }

                    model.MapTo(image);
                });
        }

        public async Task RefreshAsync<TEntity, TModel, TModelData>(Func<FluffleSearchContext, DbSet<TEntity>> getSet,
            Func<long, Task<TModel>> getModel, Func<FluffleSearchContext, TModelData, Task> processAsync)
            where TEntity : class, ITrackable where TModel : ITrackableModel<TModelData>
        {
            long afterChangeId = 0;
            await UseContextResilientAsync(async context =>
            {
                if (await getSet(context).AnyAsync())
                    afterChangeId = await getSet(context).MaxAsync(i => i.ChangeId);
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
                    foreach (var modelData in model.Results)
                        await processAsync(context, modelData);

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
