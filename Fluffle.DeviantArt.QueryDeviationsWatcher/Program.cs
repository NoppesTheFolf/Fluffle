using Humanizer;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.DeviantArt.Client;
using Noppes.Fluffle.DeviantArt.Shared;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;

namespace Noppes.Fluffle.DeviantArt.QueryDeviationsWatcher;

public class Program : ScheduledService<Program>
{
    protected override TimeSpan Interval => _configuration.Interval.Seconds();

    private const int BatchEnqueueSize = 100;

    private readonly DeviantArtClient _client;
    private readonly ProcessDeviationQueue _queue;
    private readonly DeviantArtTags _tags;
    private readonly IQueryDeviationsLatestPublishedWhenStore _latestPublishedWhenStore;
    private readonly DeviantArtQueryDeviationsWatcherConfiguration _configuration;
    private readonly ILogger<Program> _logger;

    public Program(IServiceProvider services, DeviantArtClient client, ProcessDeviationQueue queue,
        DeviantArtTags tags, IQueryDeviationsLatestPublishedWhenStore latestPublishedWhenStore,
        DeviantArtQueryDeviationsWatcherConfiguration configuration, ILogger<Program> logger) : base(services)
    {
        _client = client;
        _queue = queue;
        _tags = tags;
        _latestPublishedWhenStore = latestPublishedWhenStore;
        _configuration = configuration;
        _logger = logger;
    }

    private static async Task Main(string[] args) => await RunAsync(args, (conf, services) =>
    {
        services.AddDeviantArt(conf, x => x.QueryDeviationsWatcher, false, true, true, true);
    });

    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        // Browse the newest deviations with queries using tags we know are related to the furry fandom
        var results = new List<QueryResult>();
        foreach (var tag in _tags)
        {
            var retrievedPreviouslyWhen = (await _latestPublishedWhenStore.GetAsync(tag.Name))?.Value ?? DateTimeOffset.MinValue;

            _logger.LogInformation("Retrieving deviations using query {query}", tag.Name);
            var deviations = await _client.EnumerateBrowseNewestAsync(tag.Name)
                .Where(x => x.Tier == null)
                .TakeWhile(x => x.PublishedWhen >= retrievedPreviouslyWhen)
                .ToListAsync();
            _logger.LogInformation("Retrieved {count} deviations using query {query}", deviations.Count, tag.Name);

            if (!deviations.Any())
                continue;

            var result = new QueryResult(tag.Name, deviations);
            results.Add(result);
        }

        // It is very much possible the same deviation was retrieved more than once using
        // different queries. We filter out the duplicates for the sake of efficiency
        var totalCount = results.Select(x => x.Deviations).Sum(x => x.Count);
        _logger.LogInformation("Retrieved a total of {count} deviations", totalCount);

        var uniqueDeviations = results.SelectMany(x => x.Deviations).DistinctBy(x => x.Id).ToList();
        _logger.LogInformation("Retrieved a total of {count} unique deviations", uniqueDeviations.Count);

        // Submit all unique deviations in batches so logging looks a bit more... alive
        foreach (var batch in uniqueDeviations.Chunk(BatchEnqueueSize))
        {
            await _queue.EnqueueManyAsync(batch);
            _logger.LogInformation("Added {count} deviations to the queue", batch.Length);
        }

        // At last update the published time so next time we know where to stop
        foreach (var result in results)
        {
            var mostRecentDeviation = result.Deviations.OrderByDescending(x => x.PublishedWhen).First();
            await _latestPublishedWhenStore.SetAsync(result.Query, mostRecentDeviation.PublishedWhen);
            _logger.LogInformation("Updated the publishing time for query {query}", result.Query);
        }
    }
}