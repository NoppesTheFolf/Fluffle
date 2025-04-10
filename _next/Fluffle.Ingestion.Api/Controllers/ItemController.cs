using Fluffle.Ingestion.Api.Extensions;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Core.Domain.Items;
using Fluffle.Ingestion.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Ingestion.Api.Controllers;

[ApiController]
public class ItemController : ControllerBase
{
    private readonly ItemActionService _itemActionService;

    public ItemController(ItemActionService itemActionService)
    {
        _itemActionService = itemActionService;
    }

    [HttpPut("/api/item", Name = "PutItem")]
    public async Task<IActionResult> PutItemAsync([FromBody] PutItemModel model)
    {
        var item = new Item
        {
            ItemId = model.ItemId,
            Images = model.Images.Select(x => new Image
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Properties = model.Properties?.ToExpando()
        };
        await _itemActionService.EnqueueIndexAsync(item, model.Priority);

        return Accepted();
    }

    [HttpDelete("/api/item", Name = "DeleteItem")]
    public async Task<IActionResult> DeleteItemAsync(string itemId)
    {
        await _itemActionService.EnqueueDeleteAsync(itemId);

        return Accepted();
    }
}
