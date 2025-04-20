using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Visitors;
using Fluffle.Ingestion.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Ingestion.Api.Controllers;

[ApiController]
public class ItemActionsController : ControllerBase
{
    private readonly ItemActionService _itemActionService;

    public ItemActionsController(ItemActionService itemActionService)
    {
        _itemActionService = itemActionService;
    }

    [HttpPut("/item-actions", Name = "PutItemActions")]
    public async Task<IActionResult> PutItemActionsAsync(ICollection<PutItemActionModel> models)
    {
        var visitor = new ItemActionServiceVisitor(_itemActionService);
        foreach (var model in models)
        {
            await model.Visit(visitor);
        }

        return Accepted();
    }

    [HttpGet("/item-actions/dequeue", Name = "DequeueItemAction")]
    public async Task<IActionResult> DequeueItemActionAsync()
    {
        var itemAction = await _itemActionService.DequeueAsync();

        if (itemAction == null)
            return NoContent();

        var model = itemAction.Visit(new ModelMapperItemActionVisitor());
        return Ok(model);
    }

    [HttpPost("/item-actions/{itemActionId}/acknowledge", Name = "AcknowledgeItemAction")]
    public async Task<IActionResult> AcknowledgeItemActionAsync(string itemActionId)
    {
        await _itemActionService.AcknowledgeAsync(itemActionId);

        return Ok();
    }
}
