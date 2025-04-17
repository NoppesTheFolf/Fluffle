using Fluffle.Vector.Api.Extensions;
using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Repositories;
using Fluffle.Vector.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fluffle.Vector.Api.Controllers;

[ApiController]
public class ItemsController : ControllerBase
{
    private readonly ItemService _itemService;
    private readonly IItemRepository _itemRepository;

    public ItemsController(ItemService itemService, IItemRepository itemRepository)
    {
        _itemService = itemService;
        _itemRepository = itemRepository;
    }

    [HttpPut("/api/items/{itemId}", Name = "PutItem")]
    public async Task<IActionResult> PutItemAsync(string itemId, [FromBody] PutItemModel model)
    {
        await _itemRepository.UpsertAsync(new Item
        {
            ItemId = itemId,
            Images = model.Images.Select(x => new Image
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Properties = model.Properties.ToExpando()
        });

        return Ok();
    }

    [HttpGet("/api/items/{itemId}", Name = "GetItem")]
    public async Task<IActionResult> GetItemAsync(string itemId)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        return Ok(new ItemModel
        {
            ItemId = item.ItemId,
            Images = item.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Properties = JsonSerializer.SerializeToNode(item.Properties) ??
                         throw new InvalidOperationException("Item properties should never serialize to null.")
        });
    }

    [HttpDelete("/api/items/{itemId}", Name = "DeleteItem")]
    public async Task<IActionResult> DeleteItemAsync(string itemId)
    {
        await _itemService.DeleteAsync(itemId);

        return Ok();
    }

    [HttpPut("/api/items/{itemId}/vectors/{modelId}", Name = "PutItemVectors")]
    public async Task<IActionResult> PutItemVectorsAsync(string itemId, string modelId, [FromBody] ICollection<PutItemVectorModel> models)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        await _itemService.UpsertVectorsAsync(new ItemVectors
        {
            ItemVectorsId = new ItemVectorsId(itemId, modelId),
            Vectors = models.Select(x => new ItemVector
            {
                Value = x.Value
            }).ToList()
        });

        return Ok();
    }
}
