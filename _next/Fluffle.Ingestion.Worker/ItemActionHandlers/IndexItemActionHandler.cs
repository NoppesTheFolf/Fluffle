using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Worker.ItemContentClient;
using Fluffle.Ingestion.Worker.ThumbnailStorage;
using Fluffle.Vector.Api.Client;
using Fluffle.Vector.Api.Models.Items;
using System.Text.Json.Nodes;
using IngestionItemModel = Fluffle.Ingestion.Api.Models.Items.ItemModel;
using PutItemModel = Fluffle.Vector.Api.Models.Items.PutItemModel;
using ThumbnailModel = Fluffle.Vector.Api.Models.Items.ThumbnailModel;
using VectorItemModel = Fluffle.Vector.Api.Models.Items.ItemModel;

namespace Fluffle.Ingestion.Worker.ItemActionHandlers;

public class IndexItemActionHandler : IItemActionHandler
{
    private readonly IndexItemActionModel _itemAction;
    private readonly IItemContentClient _itemContentClient;
    private readonly IImagingApiClient _imagingApiClient;
    private readonly IInferenceApiClient _inferenceApiClient;
    private readonly IThumbnailStorage _thumbnailStorage;
    private readonly IVectorApiClient _vectorApiClient;
    private readonly ILogger<IndexItemActionHandler> _logger;

    public IndexItemActionHandler(IndexItemActionModel itemAction, IServiceProvider serviceProvider)
    {
        _itemAction = itemAction;
        _itemContentClient = serviceProvider.GetRequiredService<IItemContentClient>();
        _imagingApiClient = serviceProvider.GetRequiredService<IImagingApiClient>();
        _inferenceApiClient = serviceProvider.GetRequiredService<IInferenceApiClient>();
        _thumbnailStorage = serviceProvider.GetRequiredService<IThumbnailStorage>();
        _vectorApiClient = serviceProvider.GetRequiredService<IVectorApiClient>();
        _logger = serviceProvider.GetRequiredService<ILogger<IndexItemActionHandler>>();
    }

    public async Task RunAsync()
    {
        var item = _itemAction.Item;

        var existingItem = await _vectorApiClient.GetItemAsync(item.ItemId);
        if (existingItem == null)
        {
            await HandleNewItem(item);
        }
        else
        {
            await HandleExistingItem(existingItem, item);
        }
    }

    private async Task HandleExistingItem(VectorItemModel existingItem, IngestionItemModel newItem)
    {
        _logger.LogInformation("Updating item information on Vector API...");
        await _vectorApiClient.PutItemAsync(existingItem.ItemId, new PutItemModel
        {
            Images = newItem.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Thumbnail = existingItem.Thumbnail,
            Properties = newItem.Properties
        });

        _logger.LogInformation("Item has been updated!");
    }

    private async Task HandleNewItem(IngestionItemModel item)
    {
        _logger.LogInformation("Indexing new item...");

        _logger.LogInformation("Downloading image...");
        var (downloadedImage, imageStream) = await _itemContentClient.DownloadAsync(item.Images);
        await using var _ = imageStream;

        Imaging.Api.Client.ThumbnailModel thumbnail;
        if (new Uri(downloadedImage.Url).Host == "static.fluffle.xyz")
        {
            _logger.LogInformation("Detected content from legacy Fluffle. Skipping thumbnail creation.");

            using var thumbnailStream = new MemoryStream();
            await imageStream.CopyToAsync(thumbnailStream);
            await imageStream.FlushAsync();

            thumbnailStream.Position = 0;
            var thumbnailData = thumbnailStream.ToArray();

            thumbnailStream.Position = 0;
            var thumbnailMetadata = await _imagingApiClient.GetMetadataAsync(thumbnailStream);

            thumbnail = new Imaging.Api.Client.ThumbnailModel
            {
                Thumbnail = thumbnailData,
                Metadata = thumbnailMetadata
            };
        }
        else
        {
            _logger.LogInformation("Creating thumbnail...");
            thumbnail = await _imagingApiClient.CreateThumbnailAsync(imageStream, 300, 75);
        }

        _logger.LogInformation("Running inference on thumbnail...");
        float[][] vectors;
        using (var thumbnailStream = new MemoryStream(thumbnail.Thumbnail))
        {
            vectors = await _inferenceApiClient.CreateAsync([thumbnailStream]);
        }

        _logger.LogInformation("Uploading thumbnail to storage...");
        string thumbnailUrl;
        using (var thumbnailStream = new MemoryStream(thumbnail.Thumbnail))
        {
            thumbnailUrl = await _thumbnailStorage.PutAsync(item.ItemId, thumbnailStream);
        }

        _logger.LogInformation("Adding item and vectors to Vector API...");
        await _vectorApiClient.PutItemAsync(item.ItemId, new PutItemModel
        {
            Images = item.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Thumbnail = new ThumbnailModel
            {
                Width = thumbnail.Metadata.Width,
                Height = thumbnail.Metadata.Height,
                CenterX = thumbnail.Metadata.CenterX,
                CenterY = thumbnail.Metadata.CenterY,
                Url = thumbnailUrl
            },
            Properties = item.Properties
        });

        await _vectorApiClient.PutItemVectorsAsync(item.ItemId, "exactMatchV1", vectors.Select(x =>
            new PutItemVectorModel
            {
                Value = x,
                Properties = new JsonObject()
            }).ToList());

        _logger.LogInformation("Item has been indexed!");
    }
}
