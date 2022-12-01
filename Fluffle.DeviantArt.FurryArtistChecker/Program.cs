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

namespace Noppes.Fluffle.DeviantArt.FurryArtistChecker;

internal class Program : QueuePollingService<Program, CheckFurryArtistQueueItem>
{
    protected override TimeSpan Interval => _configuration.Interval.Seconds();

    private readonly DeviantArtClient _client;
    private readonly IQueue<ScrapeGalleryQueueItem> _queue;
    private readonly DeviantArtTags _tags;
    private readonly DeviantArtFurryArtistCheckerConfiguration _configuration;
    private readonly ILogger<Program> _logger;

    public Program(IServiceProvider services, DeviantArtClient client, IQueue<ScrapeGalleryQueueItem> queue,
        DeviantArtTags tags, DeviantArtConfiguration configuration, ILogger<Program> logger) : base(services)
    {
        _client = client;
        _queue = queue;
        _tags = tags;
        _configuration = configuration.FurryArtistChecker;
        _logger = logger;
    }

    private static async Task Main(string[] args) => await RunAsync(args, (conf, services) =>
    {
        services.AddDeviantArt(conf, x => x.FurryArtistChecker, true, true, true, false);
    });

    public override async Task ProcessAsync(CheckFurryArtistQueueItem value, CancellationToken cancellationToken)
    {
        var deviantId = value.Id;

        using var scope = Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<DeviantArtContext>();

        var deviant = await context.Deviants.FirstOrDefaultAsync(x => x.Id == deviantId);
        if (deviant == null)
        {
            _logger.LogWarning("No deviant with ID {id} could be found in the database.", deviantId);
            return;
        }

        deviant.IsFurryArtistDeterminedWhen = DateTime.UtcNow;

        var deviations = await _client.EnumerateBrowseGalleryAsync("all", deviant.Username).Take(_configuration.N).ToListAsync();
        if (deviations.Count < _configuration.N)
        {
            _logger.LogInformation("Deviant {username} ({id}) does not have enough ({count}) deviations in their gallery.", deviant.Username, deviant.Id, deviations.Count);

            await context.SaveChangesAsync();
            return;
        }

        // Retrieve metadata and filter on deviations containing a known furry tag
        var metadatas = await _client.GetDeviationMetadataAsync(deviations.Select(x => x.Id));
        var furryDeviations = metadatas.Values.Where(x => x.Tags.Any(y => _tags.IsFurry(y.Name) == true));

        // If enough of the retrieved deviations contain a tag relating to the furry fandom,
        // then we consider the user a furry artist
        var isFurryArtist = furryDeviations.Count() >= _configuration.NFurry;
        _logger.LogInformation("It was determined {username} ({id}) {isFurryArtist} a furry artist.", deviant.Username, deviant.Id, isFurryArtist ? "is" : "is not");

        // Regardless of whether the deviant is a furry artist, we scrape their gallery. They
        // only get scheduled for the check if they have uploaded furry art before, so their
        // gallery is likely to contain more furry(-related) art
        _logger.LogInformation("Enqueuing {username} ({id}) to get their gallery scraped.", deviant.Username, deviant.Id);
        await _queue.EnqueueAsync(new ScrapeGalleryQueueItem
        {
            Id = deviant.Id
        }, 5.Minutes(), null);

        deviant.IsFurryArtist = isFurryArtist;
        await context.SaveChangesAsync();
    }
}