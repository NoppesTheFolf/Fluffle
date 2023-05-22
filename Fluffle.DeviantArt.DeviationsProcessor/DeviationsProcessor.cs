using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.DeviantArt.Client;
using Noppes.Fluffle.DeviantArt.Client.Models;
using Noppes.Fluffle.DeviantArt.Database;
using Noppes.Fluffle.DeviantArt.Database.Entities;
using Noppes.Fluffle.DeviantArt.Shared;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Queue;
using Deviation = Noppes.Fluffle.DeviantArt.Client.Models.Deviation;

namespace Noppes.Fluffle.DeviantArt.DeviationsProcessor;

public class DeviationsProcessor
{
    private const string Platform = "DeviantArt";

    private readonly IServiceProvider _services;
    private readonly IQueue<CheckIfFurryArtistQueueItem> _userIsFurryCheckQueue;
    private readonly DeviantArtClient _deviantArtClient;
    private readonly FluffleClient _fluffleClient;
    private readonly DeviantArtTags _tags;
    private readonly DeviationsSubmitter _submitter;
    private readonly ILogger<DeviationsProcessor> _logger;
    private readonly DeviantArtDeviationsProcessorConfiguration _configuration;

    public DeviationsProcessor(IServiceProvider services, IQueue<CheckIfFurryArtistQueueItem> userIsFurryCheckQueue,
        DeviantArtClient deviantArtClient, FluffleClient fluffleClient, DeviantArtTags tags,
        DeviationsSubmitter submitter, ILogger<DeviationsProcessor> logger, DeviantArtDeviationsProcessorConfiguration configuration)
    {
        _services = services;
        _userIsFurryCheckQueue = userIsFurryCheckQueue;
        _deviantArtClient = deviantArtClient;
        _fluffleClient = fluffleClient;
        _tags = tags;
        _submitter = submitter;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task ProcessAsync(ICollection<string> deviationIds)
    {
        // Efficiently get the metadata for all deviations
        var metadatas = await _deviantArtClient.GetDeviationMetadataAsync(deviationIds);

        var processedDeviations = new List<(Deviation, DeviationMetadata)>();
        foreach (var id in deviationIds)
        {
            _logger.LogInformation("Retrieving deviation with ID {id}", id);
            var deviationResponse = await _deviantArtClient.GetDeviationAsync(id);
            if (deviationResponse.Error != null)
            {
                if (deviationResponse.Error == DeviationError.NotFound)
                {
                    _logger.LogInformation("Deviation with ID {id} does not exist. Marking for deletion at Fluffle.", id);
                    await HttpResiliency.RunAsync(() => _fluffleClient.DeleteContentAsync(Platform, new[] { id }));
                }
                else
                {
                    _logger.LogInformation("Retrieving deviation with ID {id} resulted in an {errorType} error.", id, deviationResponse.Error);
                }

                continue;
            }
            var deviation = deviationResponse.Value!;

            // Ignore deviations which are only visible to watchers or are paid
            if (deviation.PremiumFolderData != null)
            {
                _logger.LogInformation("Deviation with ID {id} is locked behind the following wall: {type}.", deviation.Id, deviation.PremiumFolderData.Type);
                continue;
            }

            var metadata = metadatas[id];
            var tagNames = metadata.Tags.Select(x => x.Name);
            var tagNamesIsFurry = tagNames.Select(_tags.IsFurry).ToList();

            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<DeviantArtContext>();
            var deviant = await context.Deviants.FirstOrDefaultAsync(x => x.Id == deviation.Author.Id);

            if (tagNamesIsFurry.All(x => x == null) && deviant?.IsFurryArtist != true)
            {
                _logger.LogInformation("Deviation with ID {id} did not contain any recognized index worthy tags and the author is not considered a furry artist.", deviation.Id);
                continue;
            }

            // We track all deviants regardless if they're furry artists or not
            if (deviant == null)
            {
                var profileResponse = await _deviantArtClient.GetProfileAsync(deviation.Author.Username);
                if (profileResponse.Error != null)
                {
                    _logger.LogInformation("Retrieving deviant with username {username} resulted in a {errorType} error.", deviation.Author.Username, profileResponse.Error);
                    continue;
                }
                var profile = profileResponse.Value!;

                deviant = new Deviant
                {
                    Id = profile.Id,
                    Username = profile.Username,
                    IconLocation = profile.IconLocation,
                    JoinedWhen = profile.Details!.JoinedWhen.UtcDateTime
                };
                _logger.LogInformation("Adding deviant {username} with ID {id}.", deviant.Username, deviant.Id);
                await context.Deviants.AddAsync(deviant);

                await context.SaveChangesAsync();
            }

            var activeFor = DateTime.UtcNow.Subtract(deviant.JoinedWhen);
            if (activeFor < _configuration.AtLeastActiveFor.Days())
            {
                _logger.LogInformation("Deviant {id} ({username}) has not had an account for long enough to be eligible for indexing.", deviant.Id, deviant.Username);
                continue;
            }

            // It could happen that a deviant first posted a deviation without furry tags and later
            // one with furry tags
            if (deviant.IsFurryArtistEnqueuedWhen == null && tagNamesIsFurry.Any(x => x == true))
            {
                _logger.LogInformation("Enqueuing deviant {deviantUsername} for the furry artist check because deviation with ID {deviationId} contains a tag known to relate to the furry fandom.", deviant.Username, deviation.Id);
                deviant.IsFurryArtistEnqueuedWhen = DateTime.UtcNow;
                await _userIsFurryCheckQueue.EnqueueAsync(new CheckIfFurryArtistQueueItem
                {
                    Id = deviant.Id
                }, null, 5.Minutes(), null);

                await context.SaveChangesAsync();
            }

            processedDeviations.Add((deviation, metadata));
        }

        await _submitter.SubmitAsync(processedDeviations);
    }
}
