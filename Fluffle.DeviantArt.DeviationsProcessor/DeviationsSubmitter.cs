using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.DeviantArt.Client.Models;
using Noppes.Fluffle.DeviantArt.Database;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;

namespace Noppes.Fluffle.DeviantArt.DeviationsProcessor;

public class DeviationsSubmitter
{
    private const string Platform = "DeviantArt";

    private readonly IServiceProvider _services;
    private readonly FluffleClient _fluffleClient;
    private readonly ILogger<DeviationsSubmitter> _logger;

    public DeviationsSubmitter(IServiceProvider services, FluffleClient fluffleClient, ILogger<DeviationsSubmitter> logger)
    {
        _services = services;
        _fluffleClient = fluffleClient;
        _logger = logger;
    }

    public async Task SubmitAsync(ICollection<(Deviation deviation, DeviationMetadata metadata)> deviations)
    {
        _logger.LogInformation("Adding {count} deviations to the database.", deviations.Count);
        await UpsertDeviationsAsync(deviations);

        _logger.LogInformation("Submitting {count} deviations to Fluffle.", deviations.Count);
        var models = deviations
            .Select(x => DeviationToContentModel(x.deviation, x.metadata))
            .Where(x => x != null)
            .ToList();
        await HttpResiliency.RunAsync(() => _fluffleClient.PutContentAsync(Platform, models));
    }

    private async Task UpsertDeviationsAsync(IEnumerable<(Deviation, DeviationMetadata)> deviations)
    {
        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<DeviantArtContext>();

        foreach (var (deviation, metadata) in deviations)
        {
            var dbDeviation = await context.Deviations.FirstOrDefaultAsync(x => x.Id == deviation.Id);
            if (dbDeviation == null)
            {
                dbDeviation = new Database.Entities.Deviation
                {
                    Id = deviation.Id,
                    DeviantId = deviation.Author.Id
                };

                await context.Deviations.AddAsync(dbDeviation);
            }

            dbDeviation.ProcessedAt = DateTime.UtcNow;
            dbDeviation.Location = deviation.Url;
            dbDeviation.Title = deviation.Title;
            dbDeviation.Tags = metadata.Tags.Select(x => x.Name).ToArray();
        }

        await context.SaveChangesAsync();
    }

    private static MediaTypeConstant GetMediaType(Deviation deviation)
    {
        if (deviation.Flash != null)
            return MediaTypeConstant.Other;

        if (deviation.Videos != null)
            return MediaTypeConstant.Video;

        var fileFormat = GetFileFormat(deviation.Content!);
        var mediaType = fileFormat switch
        {
            FileFormatConstant.Jpeg => MediaTypeConstant.Image,
            FileFormatConstant.Png => MediaTypeConstant.Image,
            FileFormatConstant.Gif => MediaTypeConstant.AnimatedImage,
            _ => throw new ArgumentOutOfRangeException(nameof(deviation), fileFormat, null)
        };

        return mediaType;
    }

    private static FileFormatConstant GetFileFormat(DeviationFile file)
    {
        var location = new Uri(file.Location);
        var extension = Path.GetExtension(location.AbsolutePath);
        var format = FileFormatHelper.GetFileFormatFromExtension(extension);

        return format;
    }

    private static PutContentModel? DeviationToContentModel(Deviation deviation, DeviationMetadata metadata)
    {
        var files = new List<DeviationFile>();
        if (deviation.Thumbnails != null)
            files.AddRange(deviation.Thumbnails);

        if (deviation.Preview != null)
            files.Add(deviation.Preview);

        if (deviation.Content != null)
            files.Add(deviation.Content);

        if (deviation.Videos != null)
            files.AddRange(deviation.Videos);

        if (deviation.Flash != null)
            files.Add(deviation.Flash);

        if (!files.Any())
            return null;

        files = files.DistinctBy(x => x.Location).ToList();

        var model = new PutContentModel
        {
            IdOnPlatform = deviation.Id,
            ViewLocation = deviation.Url,
            Title = metadata.Title,
            Description = metadata.Description,
            Rating = metadata.IsMature ? ContentRatingConstant.Explicit : ContentRatingConstant.Safe,
            MediaType = GetMediaType(deviation),
            Priority = (int)metadata.Stats!.Views!,
            Files = files.Select(x =>
            {
                var (width, height) = x is IDeviationFileResolution resolution
                    ? (resolution.Width, resolution.Height)
                    : (-1, -1);

                return new PutContentModel.FileModel
                {
                    Location = x.Location,
                    Width = width,
                    Height = height,
                    Format = GetFileFormat(x)
                };
            }).ToList(),
            CreditableEntities = new List<PutContentModel.CreditableEntityModel>
            {
                new()
                {
                    Id = metadata.Author.Id,
                    Name = metadata.Author.Username,
                    Type = CreditableEntityType.Owner
                }
            },
            OtherSources = null,
            Source = null,
            SourceVersion = null,
            ShouldBeIndexed = true
        };

        return model;
    }
}
