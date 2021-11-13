using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class FillMissingFromTimelineIfArtist<T> : Consumer<T> where T : IUserTweetsSupplierData
    {
        private readonly IServiceProvider _services;

        public FillMissingFromTimelineIfArtist(IServiceProvider services)
        {
            _services = services;
        }

        public override async Task<T> ConsumeAsync(T data)
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();
            var user = await context.Users.FirstAsync(u => u.Id == data.Id);

            if (user.IsFurryArtist == true)
            {
                data.TimelineRetrievedAt = DateTimeOffset.UtcNow;
                await data.Timeline.FillMissingAsync();
            }

            return data;
        }
    }
}
