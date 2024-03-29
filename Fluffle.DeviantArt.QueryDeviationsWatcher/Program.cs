﻿using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.DeviantArt.Client;
using Noppes.Fluffle.DeviantArt.Shared;
using Noppes.Fluffle.Service;

namespace Noppes.Fluffle.DeviantArt.QueryDeviationsWatcher;

public class Program : ScheduledService<Program>
{
    protected override TimeSpan Interval => _configuration.Interval;

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

    private static async Task Main(string[] args) => await RunAsync(args, "DeviantArtQueryDeviationsWatcher", (conf, services) =>
    {
        services.AddDeviantArt(conf, x => x.QueryDeviationsWatcher, false, true, true, true);
    });

    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        // Retrieve a dictionary containing the search phrases and the time of latest deviations previously retrieved
        var latestPublishedWhenPerQuery = (await _latestPublishedWhenStore.GetAsync())?.Value ?? new Dictionary<string, DateTimeOffset>();

        // Browse the newest deviations with queries using tags we know are related to the furry fandom
        var results = new List<QueryResult>();
        foreach (var tag in _tags)
        {
            if (!latestPublishedWhenPerQuery.TryGetValue(tag.Name, out var retrievedPreviouslyWhen))
                retrievedPreviouslyWhen = DateTimeOffset.MinValue;

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
            latestPublishedWhenPerQuery[result.Query] = mostRecentDeviation.PublishedWhen;
        }

        await _latestPublishedWhenStore.SetAsync(latestPublishedWhenPerQuery);
        _logger.LogInformation("Updated the publishing time for all queries");
    }
}
