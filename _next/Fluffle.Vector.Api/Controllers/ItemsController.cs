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
    private readonly IModelRepository _modelRepository;

    public ItemsController(
        IItemRepository itemRepository,
        IItemVectorsRepository itemVectorsRepository,
        IModelRepository modelRepository)
    {
        _itemRepository = itemRepository;
        _itemVectorsRepository = itemVectorsRepository;
        _modelRepository = modelRepository;
    }

    [HttpPut("/items/{itemId}", Name = "PutItem")]
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

    [HttpGet("/items/{itemId}", Name = "GetItem")]
    public async Task<IActionResult> GetItemAsync(string itemId)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        return Ok(item.ToModel());
    }

    [HttpGet("/items", Name = "GetItems")]
    public async Task<IActionResult> GetItemsAsync([FromQuery] ICollection<string> itemIds)
    {
        var items = await _itemRepository.GetAsync(itemIds);

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

    [HttpPut("/items/{itemId}/vectors/{modelId}", Name = "PutItemVectors")]
    public async Task<IActionResult> PutItemVectorsAsync(string itemId, string modelId, [FromBody] ICollection<PutItemVectorModel> models)
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item == null)
            return NotFound($"No item with ID '{itemId}' could be found.");

        var model = await _modelRepository.GetAsync(modelId);
        if (model == null)
            return NotFound($"No model with ID '{modelId}' could be found.");

        if (models.Any(x => x.Value.Length != model.VectorDimensions))
            return BadRequest($"Query length of at least one vector does not equal expected vector length of model ({model.VectorDimensions}).");

        await _itemVectorsRepository.UpsertAsync(model, item, models.Select(x => new ItemVector
        {
            Value = x.Value,
            Properties = x.Properties.ToExpando()
        }).ToList());

        return Ok();
    }
}
