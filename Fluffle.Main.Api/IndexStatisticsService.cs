using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api
{
    public class IndexStatisticsService : IService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<IndexStatisticsService> _logger;

        public IndexStatisticsService(IServiceProvider services, ILogger<IndexStatisticsService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FluffleContext>();

            _logger.LogInformation("Updating indexing statistics...");

            var calculatedIss = context.Content
                .GroupBy(c => new { c.PlatformId, c.MediaTypeId })
                .Select(c => new
                {
                    c.Key.PlatformId,
                    c.Key.MediaTypeId,
                    Count = c.Count(),
                    IndexedCount = c.Count(c => c.IsIndexed)
                }).ToDictionary(c => (c.PlatformId, c.MediaTypeId));

            var currentIss = context.IndexStatistics
                .Include(iss => iss.Platform)
                .Include(iss => iss.MediaType);

            foreach (var currentIs in currentIss)
            {
                if (!calculatedIss.TryGetValue((currentIs.PlatformId, currentIs.MediaTypeId), out var calculatedIs))
                {
                    calculatedIs = new
                    {
                        currentIs.PlatformId,
                        currentIs.MediaTypeId,
                        Count = 0,
                        IndexedCount = 0
                    };
                }

                if (currentIs.Count != calculatedIs.Count)
                {
                    _logger.LogWarning(
                        "Calculated total indexing statistic differs (current: {current}, calculated: {calculated}) for platform {platformName} and {mediaTypeName}.",
                        currentIs.Count, calculatedIs.Count, currentIs.Platform.Name, currentIs.MediaType.Name);

                    currentIs.Count = calculatedIs.Count;
                }

                if (currentIs.IndexedCount != calculatedIs.IndexedCount)
                {
                    _logger.LogWarning(
                        "Calculated total indexing statistic differs (current: {current}, calculated: {calculated}) for platform {platformName} and {mediaTypeName}.",
                        currentIs.IndexedCount, calculatedIs.IndexedCount, currentIs.Platform.Name, currentIs.MediaType.Name);

                    currentIs.IndexedCount = calculatedIs.IndexedCount;
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Updated indexing statistics.");
        }
    }
}
