using Noppes.Fluffle.Search.Business.Repositories;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Search.Domain;
using Noppes.Fluffle.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Database.Repositories;

internal class ImageRepository : IImageRepository
{
    private readonly FluffleSearchContext _context;

    public ImageRepository(FluffleSearchContext context)
    {
        _context = context;
    }

    public async Task<IList<Image>> GetAsync(int platformId, long afterChangeId, int limit)
    {
        var imageEntities = _context.DenormalizedImages
            .Where(x => x.PlatformId == platformId && x.ChangeId > afterChangeId)
            .OrderBy(x => x.ChangeId)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.IsSfw,
                x.ChangeId,
                x.IsDeleted,
                x.PhashAverage64,
                x.PhashAverage256
            }).AsAsyncEnumerable();

        var images = new List<Image>(limit);
        await foreach (var imageEntity in imageEntities)
        {
            images.Add(new Image
            {
                Id = imageEntity.Id,
                IsSfw = imageEntity.IsSfw,
                ChangeId = imageEntity.ChangeId,
                IsDeleted = imageEntity.IsDeleted,
                PhashAverage64 = ByteConvert.ToUInt64(imageEntity.PhashAverage64),
                PhashAverage256 = ByteConvert.ToInt64(imageEntity.PhashAverage256)
            });
        }

        return images;
    }
}
