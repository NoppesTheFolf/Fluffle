using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Search.Business.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platform = Noppes.Fluffle.Search.Domain.Platform;

namespace Noppes.Fluffle.Search.Database.Repositories;

internal class PlatformRepository : IPlatformRepository
{
    private readonly FluffleSearchContext _context;

    public PlatformRepository(FluffleSearchContext context)
    {
        _context = context;
    }

    public async Task<ICollection<Platform>> GetAsync()
    {
        var platforms = await _context.Platforms
            .Select(x => new Platform
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();

        return platforms;
    }
}
