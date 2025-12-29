using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Validation;
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
    public async Task<IActionResult> PutItemActionsAsync(IList<PutItemActionModel> models)
    {
        var validator = new PutItemActionModelCollectionValidator();
        var validationResult = await validator.ValidateAsync(models);
        foreach (var validationFailure in validationResult.Errors)
            ModelState.AddModelError(validationFailure.PropertyName, validationFailure.ErrorMessage);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ids = new List<string>();
        var visitor = new ItemActionServiceVisitor(_itemActionService);
        foreach (var model in models)
        {
            var id = await model.Visit(visitor);
            ids.Add(id);
        }

        return Accepted(ids);
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
