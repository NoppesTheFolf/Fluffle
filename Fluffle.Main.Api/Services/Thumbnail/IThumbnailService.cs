using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public interface IThumbnailService
{
    async Task DeleteAsync(IEnumerable<Thumbnail> thumbnails, bool save = true)
    {
        foreach (var thumbnail in thumbnails)
            await DeleteAsync(thumbnail, save);
    }

    Task DeleteAsync(Thumbnail thumbnail, bool save = true);
}
