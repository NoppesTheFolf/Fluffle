﻿using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database;
using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using static MoreLinq.Extensions.DistinctByExtension;

namespace Noppes.Fluffle.Main.Api.Services
{
    public class ContentService : Service, IContentService
    {
        private readonly FluffleContext _context;
        private readonly TagBlacklistCollection _tagBlacklist;
        private readonly IThumbnailService _thumbnailService;
        private readonly IndexStatisticsService _indexStatisticsService;
        private readonly ChangeIdIncrementer<Content> _contentCii;
        private readonly ChangeIdIncrementer<CreditableEntity> _creditableEntityCii;
        private readonly ClaimsPrincipal _user;

        public ContentService(FluffleContext context, TagBlacklistCollection tagBlacklist, IThumbnailService thumbnailService,
            IndexStatisticsService indexStatisticsService, ChangeIdIncrementer<Content> contentCii,
            ChangeIdIncrementer<CreditableEntity> creditableEntityCii, ClaimsPrincipal user)
        {
            _context = context;
            _tagBlacklist = tagBlacklist;
            _thumbnailService = thumbnailService;
            _indexStatisticsService = indexStatisticsService;
            _contentCii = contentCii;
            _creditableEntityCii = creditableEntityCii;
            _user = user;
        }

        public async Task<SE> MarkForDeletionAsync(string platformName, string idOnPlatform, bool saveChanges = true)
        {
            return await _context.Content.NotDeleted().GetContentAsync(_context.Platforms, platformName, idOnPlatform, async content =>
            {
                content.IsMarkedForDeletion = true;

                if (saveChanges)
                    await _context.SaveChangesAsync();

                return null;
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
                    imageToDelete.IsMarkedForDeletion = true;
                    deletedIdsOnPlatform.Add((int)imageToDelete.IdOnPlatformAsInteger);
                }

                await _context.SaveChangesAsync();

                return new SR<IEnumerable<int>>(deletedIdsOnPlatform);
            });
        }

        public async Task<SE> DeleteAsync(string platformName, string idOnPlatform)
        {
            using var _ = await _indexStatisticsService.LockAsync();

            var query = _context.Content.Where(c => !c.IsDeleted)
                .Include(c => c.Platform)
                .IncludeIndexStatistics()
                .IncludeThumbnails();

            return await query.GetContentAsync(_context.Platforms, platformName, idOnPlatform, async content =>
            {
                var thumbnails = content.EnumerateThumbnails();
                await _thumbnailService.DeleteAsync(thumbnails, false);

                var statsForContentType = content.Stats();
                statsForContentType.Count--;

                if (content.IsIndexed)
                    statsForContentType.IndexedCount--;

                // Remove the image hash if the content happens to be an image
                if (content is Image image)
                {
                    var imageHash = await _context.ImageHashes.ForAsync(image);

                    if (imageHash != null)
                        _context.ImageHashes.Remove(imageHash);
                }

                content.IsIndexed = false;
                content.RequiresIndexing = true;

                if (content.ChangeId != null)
                    await _contentCii.NextAsync(content);

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
                .IncludeIndexStatistics()
                .Include(i => i.Platform);

            return await query.GetContentAsync(_context.Platforms, platformName, platformContentId, async content =>
            {
                content.Errors.Add(new Database.Models.ContentError
                {
                    Message = model.Error,
                    IsFatal = model.IsFatal
                });

                if (model.IsFatal)
                    content.IsMarkedForDeletion = true;

                await _context.SaveChangesAsync();

                return null;
            });
        }

        public async Task<SE> PutContentAsync(string platformName, IList<PutContentModel> contentModels)
        {
            return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
            {
                // Substitute null values for empty collections as those are easier to work with
                foreach (var contentModel in contentModels)
                {
                    contentModel.CreditableEntities ??= new List<PutContentModel.CreditableEntityModel>();
                    contentModel.Files ??= new List<PutContentModel.FileModel>();
                    contentModel.Tags ??= new List<string>();
                }

                var blacklistedContent = contentModels
                    .Where(cm => _tagBlacklist.Any(cm.Tags, cm.Rating))
                    .ToList();

                // Mark existing blacklisted content for deletion. Content which hasn't been added
                // will simply by ignored
                var existingBlacklistedContent = _context.Content
                    .Where(c => c.PlatformId == platform.Id)
                    .Where(c => blacklistedContent.Select(bc => bc.IdOnPlatform).Contains(c.IdOnPlatform));

                foreach (var existingBlacklistedContentPiece in existingBlacklistedContent)
                    if (!existingBlacklistedContentPiece.IsDeleted)
                        existingBlacklistedContentPiece.IsMarkedForDeletion = true;

                // Get all of the models which don't contained blacklisted tags
                contentModels = contentModels.Except(blacklistedContent).ToList();

                var modelLookup = contentModels
                    .ToDictionary(c => c.IdOnPlatform, c => c);

                // First we synchronize creditable entities
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

                var creditableEntitiesSynchronizeResult = await _context.SynchronizeCreditableEntitiesAsync(
                    existingCreditableEntities.Values, creditableEntities, _creditableEntityCii);

                var creditableEntitiesLookup = creditableEntitiesSynchronizeResult.Entities()
                    .ToDictionary(ce => ce.IdOnPlatform);

                // Then we synchronize the tags. Some different tags might be normalized to the same
                // string. To prevent constraint violations, we remove duplicates
                foreach (var model in contentModels)
                    model.Tags = model.Tags.Select(TagHelper.Normalize).Distinct().ToList();

                var tags = contentModels
                    .SelectMany(c => c.Tags)
                    .Distinct()
                    .Select(t => new Tag
                    {
                        Name = t
                    })
                    .ToList();

                var existingTags = await _context.Tags
                    .Where(t => tags.Select(t => t.Name).Contains(t.Name))
                    .ToDictionaryAsync(t => t.Name);

                foreach (var tag in tags)
                    if (existingTags.TryGetValue(tag.Name, out var dbTag))
                        tag.Id = dbTag.Id;

                var tagsSynchronizeResult = await _context.SynchronizeTagsAsync(existingTags.Values, tags);

                var tagEntitiesLookup = tagsSynchronizeResult.Entities()
                    .ToDictionary(t => t.Name);

                // And at last we synchronize the actual content
                var content = contentModels.Select(c => c.MediaType switch
                {
                    MediaTypeConstant.Image => c.MapTo<Image>(),
                    _ => c.MapTo<Content>()
                }).Select(c =>
                {
                    c.PlatformId = platform.Id;

                    return c;
                }).ToList();

                var existingContent = await _context.Content.AsSingleQuery()
                    .Include(ec => ec.Files)
                    .Include(ec => ec.Credits)
                    .Include(ec => ec.Tags)
                    .Where(c => c.PlatformId == platform.Id && contentModels.Select(c => c.IdOnPlatform).Contains(c.IdOnPlatform))
                    .ToDictionaryAsync(c => c.IdOnPlatform, c => c); ;

                foreach (var contentPiece in content)
                    if (existingContent.TryGetValue(contentPiece.IdOnPlatform, out var existingContentPiece))
                        contentPiece.Id = existingContentPiece.Id;

                var contentSynchronizeResult = await _context.SynchronizeAsync(c => c.Content, existingContent.Values, content,
                    (c1, c2) =>
                    {
                        return c1.Id == c2.Id;
                    }, newContent =>
                    {
                        newContent.RequiresIndexing = true;
                        newContent.LastEditedById = _user.GetApiKeyId();

                        return Task.CompletedTask;
                    }, (src, dest) =>
                    {
                        dest.IdOnPlatform = src.IdOnPlatform;
                        dest.IdOnPlatformAsInteger = src.IdOnPlatformAsInteger;
                        dest.ViewLocation = src.ViewLocation;
                        dest.RatingId = src.RatingId;
                        dest.Title = src.Title;
                        dest.MediaTypeId = src.MediaTypeId;

                        // Some images might have failed to be downloaded. We therefore need to
                        // resume them if they're submitted again.
                        dest.IsMarkedForDeletion = false;
                        dest.IsDeleted = false;

                        return Task.CompletedTask;
                    }, updateAnywayAsync: (src, dest) =>
                    {
                        dest.Priority = src.Priority;
                        dest.LastEditedById = _user.GetApiKeyId();

                        return Task.CompletedTask;
                    });

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

                    var contentTags = contentModel.Tags.Select(t => new ContentTag
                    {
                        Content = contentPiece,
                        Tag = tagEntitiesLookup[t]
                    }).ToList();
                    var synchronizeTagsResult = await _context.SynchronizeContentTagsAsync(contentPiece.ContentTags, contentTags);

                    var isContentChanged = synchronizeResult.HasChanges || synchronizeFilesResult.HasChanges || synchronizeCredits.HasChanges;

                    if (contentPiece.ChangeId != null && isContentChanged)
                        await _contentCii.NextAsync(contentPiece);
                }

                using var _ = await _indexStatisticsService.LockAsync();

                var indexStatisticsLookup = await _context.IndexStatistics
                    .Where(s => s.PlatformId == platform.Id)
                    .ToDictionaryAsync(s => s.MediaTypeId, s => s);

                var nonDeletedMediaTypeGroups = contentSynchronizeResult.Added
                    .GroupBy(c => c.Entity.MediaTypeId);

                foreach (var mediaTypeGrouping in nonDeletedMediaTypeGroups)
                {
                    var statistic = indexStatisticsLookup[mediaTypeGrouping.Key];

                    statistic.Count += mediaTypeGrouping.Count();
                }

                await _context.SaveChangesAsync();

                return null;
            });
        }

        public async Task<SR<IEnumerable<UnprocessedContentModel>>> GetUnprocessedImages(string platformName)
        {
            return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
            {
                var models = await _context.GetUnprocessedAsync(c => c.Images, platform);

                return new SR<IEnumerable<UnprocessedContentModel>>(models);
            });
        }

        public async Task<SR<int?>> GetMaxIdOnPlatform(string platformName)
        {
            return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
            {
                var maxId = await _context.Content
                    .Where(i => i.Platform.Id == platform.Id)
                    .MaxAsync(i => i.IdOnPlatformAsInteger);

                return new SR<int?>(maxId);
            });
        }
    }
}