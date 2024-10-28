using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Database;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public class ContentService : Service, IContentService
{
    private static readonly IDictionary<PlatformConstant, AsyncLock> PlatformSyncLocks = Enum.GetValues<PlatformConstant>()
        .ToDictionary(x => x, _ => new AsyncLock());

    private readonly FluffleContext _context;
    private readonly IThumbnailService _thumbnailService;
    private readonly ChangeIdIncrementer<Content> _contentCii;
    private readonly ChangeIdIncrementer<CreditableEntity> _creditableEntityCii;
    private readonly ClaimsPrincipal _user;
    private readonly ILogger<ContentService> _logger;

    public ContentService(
        FluffleContext context,
        IThumbnailService thumbnailService,
        ChangeIdIncrementer<Content> contentCii,
        ChangeIdIncrementer<CreditableEntity> creditableEntityCii,
        ClaimsPrincipal user,
        ILogger<ContentService> logger)
    {
        _context = context;
        _thumbnailService = thumbnailService;
        _contentCii = contentCii;
        _creditableEntityCii = creditableEntityCii;
        _user = user;
        _logger = logger;
    }

    public async Task<SR<IEnumerable<string>>> GetContentByReferences(string platformName, IEnumerable<string> references)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var ids = await _context.Content
                .Where(x => x.PlatformId == platform.Id)
                .Where(x => references.Contains(x.Reference))
                .Select(x => x.IdOnPlatform)
                .ToListAsync();

            return new SR<IEnumerable<string>>(ids);
        });
    }

    public async Task<SR<IEnumerable<string>>> MarkManyForDeletionAsync(string platformName, IEnumerable<string> idsOnPlatform, bool saveChanges = true)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var contents = await _context.Content
                .Where(x => x.PlatformId == platform.Id)
                .Where(x => idsOnPlatform.Contains(x.IdOnPlatform))
                .ToListAsync();

            foreach (var content in contents)
            {
                content.HasFatalErrors = false;

                if (!content.IsDeleted)
                    content.IsMarkedForDeletion = true;
            }

            if (saveChanges)
                await _context.SaveChangesAsync();

            return new SR<IEnumerable<string>>(contents.Select(x => x.IdOnPlatform).ToList());
        });
    }

    public async Task<SR<IEnumerable<int>>> MarkRangeForDeletionAsync(string platformName, DeleteContentRangeModel model)
    {
        var deletedIdsOnPlatform = new List<int>();

        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var imagesToDelete = _context.Content
                .Where(i => i.PlatformId == platform.Id)
                .Where(i => i.IdOnPlatformAsInteger > model.ExclusiveStart && i.IdOnPlatformAsInteger <= model.InclusiveEnd)
                .Where(i => !i.IsMarkedForDeletion && !i.IsDeleted) // Exclude those already marked for deletion or are already deleted
                .Where(i => !model.ExcludedIds.Select(ei => (int?)ei).Contains(i.IdOnPlatformAsInteger));

            foreach (var imageToDelete in imagesToDelete)
            {
                imageToDelete.HasFatalErrors = false;
                imageToDelete.IsMarkedForDeletion = true;
                deletedIdsOnPlatform.Add((int)imageToDelete.IdOnPlatformAsInteger);
            }

            await _context.SaveChangesAsync();

            return new SR<IEnumerable<int>>(deletedIdsOnPlatform);
        });
    }

    public async Task<SE> DeleteAsync(string platformName, string idOnPlatform)
    {
        var query = _context.Content
            .Include(c => c.Platform)
            .IncludeThumbnails();

        return await query.GetContentAsync(_context.Platforms, platformName, idOnPlatform, async content =>
        {
            var thumbnails = content.EnumerateThumbnails();
            await _thumbnailService.DeleteAsync(thumbnails, false);

            // Remove the image hash if the content happens to be an image
            if (content is Image image)
            {
                var imageHash = await _context.ImageHashes.ForAsync(image);

                if (imageHash != null)
                    _context.ImageHashes.Remove(imageHash);
            }

            content.IsIndexed = false;
            content.RequiresIndexing = true;

            using var _ = _contentCii.Lock((PlatformConstant)content.PlatformId, out var contentCii);

            if (content.ChangeId != null)
                contentCii.Next(content);

            // Switch for marked for deletion to actually deleted
            content.IsMarkedForDeletion = false;
            content.IsDeleted = true;

            await _context.SaveChangesAsync();

            return null;
        });
    }

    public async Task<SE> PutWarningAsync(string platformName, string platformContentId, PutWarningModel model)
    {
        return await _context.Content.GetContentAsync(_context.Platforms, platformName, platformContentId, async content =>
        {
            content.Warnings.Add(new ContentWarning
            {
                Message = model.Warning
            });

            await _context.SaveChangesAsync();

            return null;
        });
    }

    public async Task<SE> PutErrorAsync(string platformName, string platformContentId, PutErrorModel model)
    {
        var query = _context.Content
            .Include(i => i.Platform);

        return await query.GetContentAsync(_context.Platforms, platformName, platformContentId, async content =>
        {
            content.Errors.Add(new Database.Models.ContentError
            {
                Message = model.Error,
                IsFatal = model.IsFatal
            });

            if (model.IsFatal)
            {
                content.HasFatalErrors = true;
                content.IsMarkedForDeletion = true;
            }

            await _context.SaveChangesAsync();

            return null;
        });
    }

    public async Task<SE> PutContentAsync(string platformName, IList<PutContentModel> contentModels)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            // Clean up the submitted models
            foreach (var contentModel in contentModels)
            {
                // Substitute null values for empty collections as those are easier to work with
                contentModel.CreditableEntities ??= new List<PutContentModel.CreditableEntityModel>();
                contentModel.Files ??= new List<PutContentModel.FileModel>();

                // Remove NULL characters from user provided data as PostgreSQL does not support it
                contentModel.Reference = contentModel.Reference?.RemoveNullChar();
                contentModel.Title = contentModel.Title?.RemoveNullChar();
                foreach (var creditableEntity in contentModel.CreditableEntities)
                {
                    creditableEntity.Id = creditableEntity.Id.RemoveNullChar();
                    creditableEntity.Name = creditableEntity.Name.RemoveNullChar();
                }
            }

            var modelLookup = contentModels
                .ToDictionary(c => c.IdOnPlatform, c => c);

            using var platformLock = await PlatformSyncLocks[(PlatformConstant)platform.Id].LockAsync();

            // Then we synchronize creditable entities
            var creditableEntities = contentModels
                .Where(c => c.CreditableEntities != null)
                .SelectMany(c => c.CreditableEntities)
                .DistinctBy(ce => ce.Id)
                .Select(ce => new CreditableEntity
                {
                    IdOnPlatform = ce.Id,
                    Name = ce.Name,
                    Type = ce.Type,
                    Platform = platform,
                })
                .ToList();

            var existingCreditableEntities = await _context.CreditableEntities
                .Where(ce => ce.PlatformId == platform.Id)
                .Where(dbce => creditableEntities.Select(ce => ce.IdOnPlatform).Contains(dbce.IdOnPlatform))
                .ToDictionaryAsync(ce => ce.IdOnPlatform, ce => ce);

            foreach (var creditableEntity in creditableEntities)
                if (existingCreditableEntities.TryGetValue(creditableEntity.IdOnPlatform, out var existingCreditableEntity))
                    creditableEntity.Id = existingCreditableEntity.Id;

            using var creditableEntityCiiLock = _creditableEntityCii.Lock((PlatformConstant)platform.Id, out var creditableEntityCii);
            var creditableEntitiesSynchronizeResult = await _context.SynchronizeCreditableEntitiesAsync(
                existingCreditableEntities.Values, creditableEntities, creditableEntityCii);

            var creditableEntitiesLookup = creditableEntitiesSynchronizeResult.Entities()
                .ToDictionary(ce => ce.IdOnPlatform);

            // And at last we synchronize the actual content
            var content = contentModels.Select(c => c.MediaType switch
            {
                MediaTypeConstant.Image => c.MapTo<Image>(),
                MediaTypeConstant.AnimatedImage => c.MapTo<Image>(),
                _ => c.MapTo<Content>()
            }).Select(c =>
            {
                c.PlatformId = platform.Id;

                return c;
            }).ToList();

            var existingContent = await _context.Content.AsSingleQuery()
                .Include(ec => ec.Files)
                .Include(ec => ec.Credits)
                .Where(c => c.PlatformId == platform.Id && contentModels.Select(c => c.IdOnPlatform).Contains(c.IdOnPlatform))
                .ToDictionaryAsync(c => c.IdOnPlatform, c => c);

            foreach (var contentPiece in content)
                if (existingContent.TryGetValue(contentPiece.IdOnPlatform, out var existingContentPiece))
                    contentPiece.Id = existingContentPiece.Id;

            var contentSynchronizeResult = await _context.SynchronizeAsync(c => c.Content, existingContent.Values, content,
                (c1, c2) =>
                {
                    return c1.Id == c2.Id;
                }, newContent =>
                {
                    newContent.LastEditedById = _user.GetApiKeyId();

                    return Task.CompletedTask;
                }, (src, dest) =>
                {
                    dest.IdOnPlatform = src.IdOnPlatform;
                    dest.IdOnPlatformAsInteger = src.IdOnPlatformAsInteger;
                    dest.ViewLocation = src.ViewLocation;
                    dest.RatingId = src.RatingId;
                    dest.MediaTypeId = src.MediaTypeId;

                    // Some images might have failed to be downloaded. We therefore need to
                    // resume them if they're submitted again.
                    dest.HasFatalErrors = false;
                    dest.IsMarkedForDeletion = false;
                    dest.IsDeleted = false;

                    return Task.CompletedTask;
                }, updateAnywayAsync: (src, dest) =>
                {
                    dest.Reference = src.Reference;
                    dest.Title = src.Title;
                    dest.Priority = src.Priority;
                    dest.LastEditedById = _user.GetApiKeyId();

                    return Task.CompletedTask;
                });

            using var contentCiiLock = _contentCii.Lock((PlatformConstant)platform.Id, out var contentCii);
            foreach (var synchronizeResult in contentSynchronizeResult.Results())
            {
                var contentPiece = synchronizeResult.Entity;
                var contentModel = modelLookup[contentPiece.IdOnPlatform];

                var contentFiles = contentModel.Files.Select(f => new ContentFile
                {
                    Content = contentPiece,
                    Width = f.Width,
                    Height = f.Height,
                    Location = f.Location,
                    FileFormatId = (int)f.Format
                }).ToList();
                var synchronizeFilesResult = await _context.SynchronizeFilesAsync(contentPiece.Files, contentFiles);

                var contentCreditableEntities = contentModel.CreditableEntities.Select(c => new ContentCreditableEntity
                {
                    Content = contentPiece,
                    CreditableEntity = creditableEntitiesLookup[c.Id]
                }).ToList();
                var synchronizeCredits = await _context.SynchronizeContentCreditsAsync(contentPiece.ContentCreditableEntity, contentCreditableEntities);

                var isContentChanged = synchronizeResult.HasChanges || synchronizeFilesResult.HasChanges || synchronizeCredits.HasChanges;
                if (contentPiece.ChangeId != null && isContentChanged)
                    contentCii.Next(contentPiece);

                // Transparency fix, this is only required for e621 due to them being the only
                // one flattening their images with a black background. This causes problems
                // when, for example, the background of a sketch is transparent.
                if (contentPiece.PlatformId == (int)PlatformConstant.E621 && contentPiece is Image image)
                {
                    // The image has transparency and has already been indexed, yet was not
                    // marked as having transparency when it got indexed.
                    if (contentModel.HasTransparency && image.IsIndexed && !image.HasTransparency)
                    {
                        image.RequiresIndexing = true;

                        _logger.LogInformation("Applied transparency fix to image with ID {idOnPlatform} on {platform}",
                            image.IdOnPlatform, platform.Name);
                    }

                    image.HasTransparency = contentModel.HasTransparency;
                }
            }

            await _context.SaveChangesAsync();

            return null;
        });
    }

    public async Task<SR<IEnumerable<UnprocessedImageModel>>> GetUnprocessedImages(string platformName)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var models = await _context.GetUnprocessedAsync<Image, UnprocessedImageModel>(x => x.Images, platform, mapModel:
               (image, model) =>
               {
                   // If the image to be indexed has transparency, we should only provide the
                   // indexer with formats that support transparency to ensure correct processing
                   if (image.HasTransparency)
                   {
                       model.Files = image.Files
                           .Where(x => ((FileFormatConstant)x.FileFormatId).SupportsTransparency())
                           .Select(x => new UnprocessedContentModel.FileModel
                           {
                               Width = x.Width,
                               Height = x.Height,
                               Location = x.Location
                           });
                   }

                   model.HasTransparency = image.HasTransparency;

                   var possiblyAnimatedImages = image.Files.Where(x => ((FileFormatConstant)x.FileFormatId).SupportsAnimation()).ToList();
                   if (possiblyAnimatedImages.Any())
                   {
                       model.Files = possiblyAnimatedImages.Select(x => new UnprocessedContentModel.FileModel
                       {
                           Width = x.Width,
                           Height = x.Height,
                           Location = x.Location
                       });
                   }
               });

            return new SR<IEnumerable<UnprocessedImageModel>>(models);
        });
    }

    private const int RetryIncrementThreshold = 3;
    private static readonly TimeSpan RetryReservationTime = 3.Days();

    private static readonly IDictionary<PlatformConstant, AsyncLock> ContentRetryLocks = Enum.GetValues<PlatformConstant>()
        .ToDictionary(x => x, _ => new AsyncLock());

    public async Task<SR<string>> GetContentToRetry(string platformName)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            using var _ = await ContentRetryLocks[(PlatformConstant)platform.Id].LockAsync();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var entity = await _context.Content
                .Where(i => i.PlatformId == platform.Id)
                .Where(i => i.HasFatalErrors && i.RetryIncrement < RetryIncrementThreshold)
                .Where(i => i.RetryReservedUntil < now)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            if (entity == null)
                return new SR<string>((string)null);

            entity.RetryIncrement++;
            entity.RetryReservedUntil = DateTimeOffset.UtcNow.Add(RetryReservationTime).ToUnixTimeSeconds();
            await _context.SaveChangesAsync();

            return new SR<string>(entity.IdOnPlatform);
        });
    }

    public async Task<SR<int?>> GetMinIdOnPlatform(string platformName)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var minId = await _context.Content
                .Where(c => c.PlatformId == platform.Id)
                .MinAsync(c => c.IdOnPlatformAsInteger);

            return new SR<int?>(minId);
        });
    }

    public async Task<SR<int?>> GetMaxIdOnPlatform(string platformName)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var maxId = await _context.Content
                .Where(c => c.PlatformId == platform.Id)
                .MaxAsync(c => c.IdOnPlatformAsInteger);

            return new SR<int?>(maxId);
        });
    }
}
