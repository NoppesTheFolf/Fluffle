using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Main.Database.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Database
{
    public static class ModelExtensions
    {
        public static Task<Platform> FirstOrDefaultAsync(this IQueryable<Platform> platforms, string name)
        {
            name = name.ToLowerInvariant();

            return platforms.FirstOrDefaultAsync(p => p.NormalizedName == name);
        }

        public static Task<TContent> FirstOrDefaultAsync<TContent>(this IQueryable<TContent> content,
            int platformId, string platformContentId) where TContent : Content
        {
            return content.FirstOrDefaultAsync(c => c.PlatformId == platformId && c.IdOnPlatform == platformContentId);
        }

        public static Task<ImageHash> ForAsync(this IQueryable<ImageHash> queryable, Image image)
        {
            return queryable.FirstOrDefaultAsync(ih => ih.Id == image.Id);
        }

        public static IQueryable<TContent> Where<TContent>(this IQueryable<TContent> content,
            int platformId, string platformContentId) where TContent : Content
        {
            return content.Where(i => i.PlatformId == platformId && i.IdOnPlatform == platformContentId);
        }

        public static IQueryable<TContent> NotDeleted<TContent>(this IQueryable<TContent> content) where TContent : Content
        {
            return content.Where(c => !c.IsMarkedForDeletion && !c.IsDeleted);
        }

        public static IQueryable<TContent> IncludeThumbnails<TContent>(this IQueryable<TContent> content) where TContent : Content
        {
            return content
                .Include(c => c.Thumbnail);
        }

        public static IQueryable<TTrackable> AfterChangeId<TTrackable>(this IQueryable<TTrackable> queryable,
            long afterChangeId) where TTrackable : ITrackable
        {
            return queryable.Where(i => i.ChangeId != null)
                .Where(i => i.ChangeId > afterChangeId)
                .OrderBy(i => i.ChangeId);
        }
    }
}
