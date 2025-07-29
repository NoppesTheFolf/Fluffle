using Fluffle.Imaging.Api.Client;
using Fluffle.Imaging.Api.Models;
using Fluffle.Inference.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Worker.ItemContentClient;
using Fluffle.Ingestion.Worker.Telemetry;
using Fluffle.Ingestion.Worker.ThumbnailStorage;
using Fluffle.Vector.Api.Client;
using Fluffle.Vector.Api.Models.Items;
using Microsoft.ApplicationInsights;
using System.Collections.Immutable;
using System.Net;
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
    private readonly TelemetryClient _telemetryClient;

    public IndexItemActionHandler(IndexItemActionModel itemAction, IServiceProvider serviceProvider)
    {
        _itemAction = itemAction;
        _itemContentClient = serviceProvider.GetRequiredService<IItemContentClient>();
        _imagingApiClient = serviceProvider.GetRequiredService<IImagingApiClient>();
        _inferenceApiClient = serviceProvider.GetRequiredService<IInferenceApiClient>();
        _thumbnailStorage = serviceProvider.GetRequiredService<IThumbnailStorage>();
        _vectorApiClient = serviceProvider.GetRequiredService<IVectorApiClient>();
        _logger = serviceProvider.GetRequiredService<ILogger<IndexItemActionHandler>>();
        _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
    }

    public async Task RunAsync()
    {
        var item = _itemAction.Item;

        var existingItem = await _vectorApiClient.GetItemAsync(item.ItemId).Timed(_telemetryClient, "VectorApiGetItem");
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
        _logger.LogInformation("Updating existing item...");

        _logger.LogInformation("Updating item information on Vector API...");
        await _vectorApiClient.PutItemAsync(existingItem.ItemId, new PutItemModel
        {
            GroupId = newItem.GroupId,
            Images = newItem.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Thumbnail = existingItem.Thumbnail,
            Properties = newItem.Properties
        }).Timed(_telemetryClient, "VectorApiPutItem");

        _logger.LogInformation("Item has been updated!");

        await HandleGroupDeletionsAsync(newItem);
    }

    private async Task HandleNewItem(IngestionItemModel item)
    {
        _logger.LogInformation("Indexing new item...");

        _logger.LogInformation("Downloading image...");
        var (downloadedImage, imageStream) = await _itemContentClient.DownloadAsync(item.Images).Timed(_telemetryClient, "DownloadImage");
        await using var _ = imageStream;

        byte[] thumbnail;
        ImageMetadataModel thumbnailMetadata;
        if (new Uri(downloadedImage.Url).Host == "static.fluffle.xyz")
        {
            _logger.LogInformation("Detected content from legacy Fluffle. Skipping thumbnail creation.");

            using var thumbnailStream = new MemoryStream();
            await imageStream.CopyToAsync(thumbnailStream);
            await imageStream.FlushAsync();

            thumbnailStream.Position = 0;
            thumbnail = thumbnailStream.ToArray();

            thumbnailStream.Position = 0;
            thumbnailMetadata = await _imagingApiClient.GetMetadataAsync(thumbnailStream).Timed(_telemetryClient, "ImagingApiGetMetadata");
        }
        else
        {
            _logger.LogInformation("Creating thumbnail...");
            (thumbnail, thumbnailMetadata) = await _imagingApiClient.CreateThumbnailAsync(imageStream, size: 300, quality: 75, calculateCenter: true).Timed(_telemetryClient, "ImagingApiCreateThumbnail");
        }

        _logger.LogInformation("Running V2 inference on thumbnail...");
        float[][] v2Vectors;
        using (var thumbnailStream = new MemoryStream(thumbnail))
        {
            v2Vectors = await _inferenceApiClient.ExactMatchV2Async([thumbnailStream]).Timed(_telemetryClient, "InferenceApiExactMatchV2");
        }

        _logger.LogInformation("Uploading thumbnail to storage...");
        string thumbnailUrl;
        using (var thumbnailStream = new MemoryStream(thumbnail))
        {
            thumbnailUrl = await _thumbnailStorage.PutAsync(item.ItemId, thumbnailStream).Timed(_telemetryClient, "ContentApiPutThumbnail");
        }

        _logger.LogInformation("Adding item and vectors to Vector API...");
        await _vectorApiClient.PutItemAsync(item.ItemId, new PutItemModel
        {
            GroupId = item.GroupId,
            Images = item.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Thumbnail = new ThumbnailModel
            {
                Width = thumbnailMetadata.Width,
                Height = thumbnailMetadata.Height,
                CenterX = thumbnailMetadata.Center!.X,
                CenterY = thumbnailMetadata.Center.Y,
                Url = thumbnailUrl
            },
            Properties = item.Properties
        }).Timed(_telemetryClient, "VectorApiPutItem");

        await _vectorApiClient.PutItemVectorsAsync(item.ItemId, "exactMatchV2", v2Vectors.Select(x =>
            new PutItemVectorModel
            {
                Value = x,
                Properties = new JsonObject()
            }).ToList()).Timed(_telemetryClient, "VectorApiPutItemVectorsExactMatchV2");

        _logger.LogInformation("Item has been indexed!");

        await HandleGroupDeletionsAsync(item);
    }

    private async Task HandleGroupDeletionsAsync(IngestionItemModel item)
    {
        if (item.GroupId == null || item.GroupItemIds == null || item.GroupItemIds.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing group with ID {GroupId}...", item.GroupId);
        var existingItems = await _vectorApiClient.GetItemsAsync(
            itemIds: null,
            groupId: item.GroupId
        ).Timed(_telemetryClient, "VectorApiGetItems");
        var existingItemIds = existingItems.Select(x => x.ItemId).ToImmutableHashSet(StringComparer.Ordinal);

        var deletedItemIds = existingItemIds.Except(item.GroupItemIds);
        if (deletedItemIds.Count == 0)
        {
            _logger.LogInformation("No items in group with ID {GroupId} need to be deleted.", item.GroupId);
            return;
        }

        // Multiple workers can be processing the same group, so it might happen another worker deleted an item
        // before this worker got the chance to
        foreach (var deletedItemId in deletedItemIds)
        {
            try
            {
                _logger.LogInformation("Deleting item with ID {ItemId} because of group with ID {GroupId}...", deletedItemId, item.GroupId);
                await _vectorApiClient.DeleteItemAsync(deletedItemId).Timed(_telemetryClient, "VectorApiDeleteItem");
            }
            catch (VectorApiException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                // Sanity check to verify the item was indeed deleted on an error
                var deletedItem = await _vectorApiClient.GetItemAsync(deletedItemId).Timed(_telemetryClient, "VectorApiGetItem");
                if (deletedItem != null)
                {
                    throw new InvalidOperationException($"Item with ID {deletedItem} was not deleted from group with ID {item.GroupId}!");
                }
            }
        }

        _logger.LogInformation("Group with ID {GroupId} has been processed!", item.GroupId);
    }
}
