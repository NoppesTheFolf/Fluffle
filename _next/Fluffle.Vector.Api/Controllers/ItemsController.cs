using Fluffle.Vector.Api.Extensions;
using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Vector.Api.Controllers;

[ApiController]
public class ItemsController : ControllerBase
{
    private readonly IItemRepository _itemRepository;
    private readonly IItemVectorsRepository _itemVectorsRepository;
    private readonly ICollectionRepository _collectionRepository;

    public ItemsController(
        IItemRepository itemRepository,
        IItemVectorsRepository itemVectorsRepository,
        ICollectionRepository collectionRepository)
    {
        _itemRepository = itemRepository;
        _itemVectorsRepository = itemVectorsRepository;
        _collectionRepository = collectionRepository;
    }

    [HttpPut("/items/{itemId}", Name = "PutItem")]
    public async Task<IActionResult> PutItemAsync(string itemId, [FromBody] PutItemModel model)
    {
        await _itemRepository.UpsertAsync(new Item
        {
            ItemId = itemId,
            GroupId = model.GroupId,
            Images = model.Images.Select(x => new Image
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Thumbnail = model.Thumbnail == null ? null : new Thumbnail
            {
                Width = model.Thumbnail.Width,
                Height = model.Thumbnail.Height,
                CenterX = model.Thumbnail.CenterX,
                CenterY = model.Thumbnail.CenterY,
                Url = model.Thumbnail.Url
            },
            Properties = model.Properties.ToExpando()
        });

        return Ok();
    }

    [HttpGet("/items/{itemId}", Name = "GetItem")]
    public async Task<IActionResult> GetItemAsync(string itemId)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        return Ok(item.ToModel());
    }

    [HttpGet("/items/{itemId}/collections", Name = "GetItemCollections")]
    public async Task<IActionResult> GetItemCollectionsAsync(string itemId)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        var collections = await _itemVectorsRepository.GetCollectionsAsync(itemId);
        return Ok(collections);
    }

    [HttpGet("/items", Name = "GetItems")]
    public async Task<IActionResult> GetItemsAsync([FromQuery] ICollection<string> itemIds, string? groupId)
    {
        var items = await _itemRepository.GetAsync(itemIds.Count == 0 ? null : itemIds, groupId);

        var models = items.Select(x => x.ToModel()).ToList();
        return Ok(models);
    }

    [HttpDelete("/items/{itemId}", Name = "DeleteItem")]
    public async Task<IActionResult> DeleteItemAsync(string itemId)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        await _itemVectorsRepository.DeleteAsync(itemId);
        await _itemRepository.DeleteAsync(itemId);

        return Ok();
    }

    [HttpPut("/items/{itemId}/vectors/{collectionId}", Name = "PutItemVectors")]
    public async Task<IActionResult> PutItemVectorsAsync(string itemId, string collectionId, [FromBody] ICollection<PutItemVectorModel> models)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        var collection = await _collectionRepository.GetAsync(collectionId);
        if (collection == null)
            return NotFound($"No collection with ID '{collectionId}' could be found.");

        if (models.Any(x => x.Value.Length != collection.VectorDimensions))
            return BadRequest($"Query length of at least one vector does not equal expected vector length of collection ({collection.VectorDimensions}).");

        await _itemVectorsRepository.UpsertAsync(collection, item, models.Select(x => new ItemVector
        {
            Value = x.Value,
            Properties = x.Properties.ToExpando()
        }).ToList());

        return Ok();
    }
}
