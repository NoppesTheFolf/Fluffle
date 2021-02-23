using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public static class TagExtensions
    {
        public static async Task<SynchronizeResult<Tag>> SynchronizeTagsAsync(this FluffleContext context, ICollection<Tag> currentTags, ICollection<Tag> newTags)
        {
            return await context.SynchronizeAsync(c => c.Tags, currentTags, newTags, (f1, f2) =>
            {
                return f1.Name == f2.Name;
            });
        }
    }
}
