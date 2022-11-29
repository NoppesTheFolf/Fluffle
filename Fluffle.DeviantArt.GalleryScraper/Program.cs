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

namespace Noppes.Fluffle.DeviantArt.GalleryScraper;

public class Program : QueuePollingService<Program, ScrapeGalleryQueueItem>
{
    protected override TimeSpan Interval => _configuration.Interval.Seconds();

    private readonly DeviantArtClient _client;
    private readonly IQueue<ProcessDeviationQueueItem> _queue;
    private readonly DeviantArtGalleryScraperConfiguration _configuration;
    private readonly ILogger<Program> _logger;

    public Program(IServiceProvider services, DeviantArtClient client, IQueue<ProcessDeviationQueueItem> queue,
        DeviantArtGalleryScraperConfiguration configuration, ILogger<Program> logger) : base(services)
    {
        _client = client;
        _queue = queue;
        _configuration = configuration;
        _logger = logger;
    }

    private static async Task Main(string[] args) => await RunAsync(args, (conf, services) =>
    {
        services.AddDeviantArt(conf, x => x.GalleryScraper, true, true, true, false);
    });

    public override async Task ProcessAsync(ScrapeGalleryQueueItem value, CancellationToken cancellationToken)
    {
        using var scope = Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<DeviantArtContext>();

        var deviant = await context.Deviants.FirstOrDefaultAsync(x => x.Id == value.Id);
        if (deviant == null)
        {
            _logger.LogWarning("No deviant with ID {id} could be found.", value.Id);
            return;
        }

        _logger.LogInformation("Retrieving deviations from the gallery of {username} ({id}).", deviant.Username, deviant.Id);
        var deviations = await _client.EnumerateBrowseGalleryAsync("all", deviant.Username).ToListAsync();
        _logger.LogInformation("Retrieved {count} deviations for {username} ({id}).", deviations.Count, deviant.Username, deviant.Id);

        var alreadyProcessedDeviationIds = await context.Deviations
            .Where(x => deviations.Select(y => y.Id).Contains(x.Id))
            .Select(x => x.Id)
            .AsAsyncEnumerable().ToHashSetAsync();

        deviations = deviations
            .Where(x => !alreadyProcessedDeviationIds.Contains(x.Id))
            .ToList();

        _logger.LogInformation("Adding {count} deviations to the queue.", deviations.Count);
        await _queue.EnqueueManyAsync(deviations.Select(x => new ProcessDeviationQueueItem
        {
            Id = x.Id
        }));

        deviant.GalleryScrapedWhen = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}