using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MoreLinq.Extensions.DistinctByExtension;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public interface IPredictClassesData : IImageRetrieverData
    {
        public ICollection<IDictionary<bool, double>> Classes { get; set; }
    }

    internal static class PredictClasses
    {
        internal static readonly AsyncLock Mutex = new();
    }

    public class PredictClasses<T> : Consumer<T> where T : IPredictClassesData
    {
        private readonly IServiceProvider _services;
        private readonly IPredictionClient _predictionClient;

        public PredictClasses(IServiceProvider services, IPredictionClient predictionClient)
        {
            _services = services;
            _predictionClient = predictionClient;
        }

        public override async Task<T> ConsumeAsync(T data)
        {
            using var _ = Operation.Time("Predicting image classes for {count} images", data.Images.Count);
            data.Classes = await HttpResiliency.RunAsync(() => _predictionClient.ClassifyAsync(data.OpenStreams));

            using var __ = await PredictClasses.Mutex.LockAsync();
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            var mediaIds = data.Images.Select(i => i.MediaId).ToList();
            var existingMediaAnalytics = await context.MediaAnalytics.Where(m => mediaIds.Contains(m.Id)).ToListAsync();
            var newMediaAnalytics = data.Classes.Zip(mediaIds).Select(x => new MediaAnalytic
            {
                Id = x.Second,
                True = x.First[true],
                False = x.First[false]
            }).DistinctBy(ma => ma.Id).ToList();

            var existingMediaIds = await context.Media
                .Where(m => mediaIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();
            newMediaAnalytics = newMediaAnalytics.Where(ma => existingMediaIds.Contains(ma.Id)).ToList();

            await context.SynchronizeAsync(c => c.MediaAnalytics, existingMediaAnalytics, newMediaAnalytics,
                (ma1, ma2) => ma1.Id == ma2.Id, onUpdateAsync: (src, dest) =>
                {
                    dest.True = src.True;
                    dest.False = src.False;

                    return Task.CompletedTask;
                });
            await context.SaveChangesAsync();

            return data;
        }
    }
}
