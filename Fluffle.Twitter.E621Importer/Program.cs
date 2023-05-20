using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Twitter.Core;
using Noppes.Fluffle.Utils;

namespace Noppes.Fluffle.Twitter.E621Importer;

internal class Program : ScheduledService<Program>
{
    protected override TimeSpan Interval => 1.5.Days();

    private static async Task Main(string[] args) => await RunAsync(args, "TwitterE621Importer", (conf, services) =>
    {
        services.AddCore(conf);

        var e621Client = new E621ClientFactory(conf).CreateAsync(E621Constants.RecommendedRequestIntervalInMilliseconds, "twitter-e621-importer").Result;
        services.AddSingleton(e621Client);

        services.AddSingleton<PostSourceRetriever>();
        services.AddSingleton<ArtistsSourceRetriever>();
        services.AddSingleton<UsernameRetriever>();
    });

    private const int BatchSize = 1024;

    private readonly UsernameRetriever _usernameRetriever;
    private readonly IQueue<ImportUserQueueItem> _importUsersQueue;

    public Program(IServiceProvider services, UsernameRetriever usernameRetriever, IQueue<ImportUserQueueItem> importUsersQueue) : base(services)
    {
        _usernameRetriever = usernameRetriever;
        _importUsersQueue = importUsersQueue;
    }

    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        // TODO: Add some more logging when extracting usernames from posts
        await foreach (var tuples in _usernameRetriever.GetUsernamesAsync().Batch(BatchSize).WithCancellation(stoppingToken))
        {
            var items = tuples.Select(tuple => new ImportUserQueueItem
            {
                Username = tuple.username,
                Source = tuple.source.ToString()
            }).ToList();
            await _importUsersQueue.EnqueueManyAsync(items, null, null, null);
        }
    }
}
