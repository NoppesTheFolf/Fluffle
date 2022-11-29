using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.DeviantArt.Client;
using Noppes.Fluffle.DeviantArt.Database;
using Noppes.Fluffle.DeviantArt.Shared;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;

namespace Noppes.Fluffle.DeviantArt.NewestDeviationsWatcher
{
    public class Program : ScheduledService<Program>
    {
        protected override TimeSpan Interval => _configuration.Interval.Seconds();

        private readonly DeviantArtClient _client;
        private readonly IQueue<ProcessDeviationQueueItem> _queue;
        private readonly INewestDeviationsLatestPublishedWhenStore _latestPublishedWhenStore;
        private readonly DeviantArtNewestDeviationsWatcherConfiguration _configuration;
        private readonly ILogger<Program> _logger;

        public Program(IServiceProvider services, DeviantArtClient client, IQueue<ProcessDeviationQueueItem> queue,
            INewestDeviationsLatestPublishedWhenStore latestPublishedWhenStore,
            DeviantArtNewestDeviationsWatcherConfiguration configuration, ILogger<Program> logger) : base(services)
        {
            _client = client;
            _queue = queue;
            _latestPublishedWhenStore = latestPublishedWhenStore;
            _configuration = configuration;
            _logger = logger;
        }

        private static async Task Main(string[] args) => await RunAsync(args, (conf, services) =>
        {
            services.AddDeviantArt(conf, x => x.NewestDeviationsWatcher, true, true, true, true);
        });

        protected override async Task RunAsync(CancellationToken stoppingToken)
        {
            var retrievedPreviouslyWhen = (await _latestPublishedWhenStore.GetAsync())?.Value ?? DateTimeOffset.MinValue;

            var deviations = await _client.EnumerateBrowseNewestAsync()
                .Where(x => x.Tier == null)
                .TakeWhile(x => x.PublishedWhen >= retrievedPreviouslyWhen)
                .ToListAsync();
            _logger.LogInformation("Retrieved {count} deviations", deviations.Count);

            var deviationsPerDeviant = deviations
                .GroupBy(x => x.Author.Id)
                .ToDictionary(x => x.Key, x => x.ToList());

            using var scope = Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<DeviantArtContext>();

            var furryDeviants = await context.Deviants.AsNoTracking()
                .Where(x => deviationsPerDeviant.Keys.Contains(x.Id))
                .Where(x => x.IsFurryArtist == true)
                .ToListAsync();

            var deviationsFromFurryDeviants = furryDeviants.SelectMany(x => deviationsPerDeviant[x.Id]).ToList();
            _logger.LogInformation("Of the retrieved {count} deviations {furryCount} are from known furry artists", deviations.Count, deviationsFromFurryDeviants.Count);

            await _queue.EnqueueManyAsync(deviationsFromFurryDeviants.Select(x => new ProcessDeviationQueueItem
            {
                Id = x.Id
            }));
            _logger.LogInformation("Added {count} deviations to the queue", deviationsFromFurryDeviants.Count);

            var mostRecentDeviation = deviations.OrderByDescending(x => x.PublishedWhen).First();
            await _latestPublishedWhenStore.SetAsync(mostRecentDeviation.PublishedWhen);
        }
    }
}