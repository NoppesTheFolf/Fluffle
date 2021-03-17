using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public class IndexService : Service, IIndexService
    {
        private readonly FluffleContext _context;
        private readonly ChangeIdIncrementer<Content> _contentCii;

        public IndexService(FluffleContext context, ChangeIdIncrementer<Content> contentCii)
        {
            _context = context;
            _contentCii = contentCii;
        }

        public Task<SE> Index(string platformName, string idOnPlatform, PutImageIndexModel model)
        {
            return PutGenericIndex(platformName, idOnPlatform, model, c => c.Images, query =>
            {
                return query
                    .Include(i => i.ImageHash);
            }, async image =>
            {
                if (image.ImageHash == null)
                {
                    var mapped = model.MapTo<ImageHash>();
                    mapped.Id = image.Id;
                    await _context.ImageHashes.AddAsync(mapped);
                }
                else
                {
                    model.MapTo(image.ImageHash);
                }
            });
        }

        private async Task<SE> PutGenericIndex<TContent, TModel>(string platformName, string idOnPlatform,
            TModel model, Func<FluffleContext, DbSet<TContent>> selectSet,
            Func<IQueryable<TContent>, IQueryable<TContent>> buildQuery, Func<TContent, Task> upsertHashAsync)
            where TModel : PutContentIndexModel where TContent : Content
        {
            var query = selectSet(_context)
                .IncludeThumbnails()
                .Include(i => i.Platform)
                .AsQueryable();

            query = buildQuery(query);

            return await query.GetContentAsync(_context.Platforms, platformName, idOnPlatform, async content =>
            {
                async Task<Thumbnail> ProcessThumbnail(Thumbnail existingThumbnail, PutContentIndexModel.ThumbnailModel thumbnailModel)
                {
                    if (existingThumbnail != null)
                        _context.Thumbnails.Remove(existingThumbnail);

                    var thumbnail = new Thumbnail
                    {
                        Width = thumbnailModel.Width,
                        Height = thumbnailModel.Height,
                        Location = thumbnailModel.Location,
                        CenterX = thumbnailModel.CenterX,
                        CenterY = thumbnailModel.CenterY,
                        Filename = thumbnailModel.Filename,
                        B2FileId = thumbnailModel.B2FileId
                    };

                    await _context.Thumbnails.AddAsync(thumbnail);
                    return thumbnail;
                }

                content.Thumbnail = await ProcessThumbnail(content.Thumbnail, model.Thumbnail);

                await upsertHashAsync(content);

                if (!content.IsIndexed)
                    content.IsIndexed = true;

                await _contentCii.NextAsync(content);
                content.RequiresIndexing = false;

                await _context.SaveChangesAsync();

                return null;
            });
        }
    }
}
