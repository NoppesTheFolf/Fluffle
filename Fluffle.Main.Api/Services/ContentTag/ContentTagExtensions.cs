using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public static class ContentTagExtensions
{
    public static async Task<SynchronizeResult<ContentTag>> SynchronizeContentTagsAsync(this FluffleContext context, ICollection<ContentTag> currentTags, ICollection<ContentTag> newTags)
    {
        return await context.SynchronizeAsync(c => c.ContentTags, currentTags, newTags, (f1, f2) =>
        {
            return (f1.Content, f1.Tag) == (f2.Content, f2.Tag);
        });
    }
}
