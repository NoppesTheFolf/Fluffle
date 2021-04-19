using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api
{
    public class IndexStatisticsService : IService, IInitializable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<IndexStatisticsService> _logger;
        private IDictionary<(int, int), (int, int)> _statistics;
        private readonly AsyncReaderWriterLock _mutex;

        public IndexStatisticsService(IServiceProvider services, ILogger<IndexStatisticsService> logger)
        {
            _services = services;
            _logger = logger;
            _mutex = new AsyncReaderWriterLock();
        }

        public (int total, int indexed) Get(int platformId, int mediaTypeId)
        {
            using var _ = _mutex.ReaderLock();

            return _statistics.TryGetValue((platformId, mediaTypeId), out var statistics) ? statistics : (0, 0);
        }

        public Task InitializeAsync() => RunAsync();

        public async Task RunAsync()
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FluffleContext>();

            _logger.LogInformation("Updating indexing statistics...");

            var statistics = await context.Content
                .Where(c => !c.IsDeleted)
                .GroupBy(c => new { c.PlatformId, c.MediaTypeId })
                .Select(cg => new
                {
                    cg.Key.PlatformId,
                    cg.Key.MediaTypeId,
                    Count = cg.Count(c => c.RequiresIndexing || c.IsIndexed),
                    IndexedCount = cg.Count(c => c.IsIndexed)
                }).ToDictionaryAsync(c => (c.PlatformId, c.MediaTypeId), c => (c.Count, c.IndexedCount));

            using var _ = await _mutex.WriterLockAsync();
            _statistics = statistics;

            _logger.LogInformation("Updated indexing statistics.");
        }
    }
}
