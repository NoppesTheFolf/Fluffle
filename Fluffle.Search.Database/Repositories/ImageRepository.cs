using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Search.Business.Repositories;
using Noppes.Fluffle.Search.Domain;
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
        var imageEntities = _context.Images
            .Where(x => x.PlatformId == platformId && x.ChangeId > afterChangeId)
            .OrderBy(x => x.ChangeId)
            .Take(limit)
            .AsAsyncEnumerable();

        var images = new List<Image>(limit);
        await foreach (var imageEntity in imageEntities)
        {
            var imageHashes = ImageHashesDeserializer.Deserialize(imageEntity.CompressedImageHashes);
            images.Add(new Image
            {
                Id = imageEntity.Id,
                IsSfw = imageEntity.IsSfw,
                ChangeId = imageEntity.ChangeId,
                IsDeleted = imageEntity.IsDeleted,
                PhashAverage64 = imageHashes.PhashAverage64,
                PhashAverage256 = imageHashes.PhashAverage256
            });
        }

        return images;
    }
}
