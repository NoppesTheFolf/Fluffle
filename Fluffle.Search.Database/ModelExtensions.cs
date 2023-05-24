using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Search.Database.Models;
using System.Linq;

namespace Noppes.Fluffle.Search.Database;

public static class ModelExtensions
{
    public static IQueryable<TContent> IncludeThumbnails<TContent>(this IQueryable<TContent> queryable) where TContent : Content
    {
        return queryable.Include(c => c.Thumbnail);
    }
}
