using Fluffle.Ingestion.Api.Extensions;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Core.Domain.Items;
using Fluffle.Ingestion.Core.Services;

namespace Fluffle.Ingestion.Api.Visitors;

public class ItemActionServiceVisitor : IPutItemActionModelVisitor<Task<string>>
{
    private readonly ItemActionService _itemActionService;

    public ItemActionServiceVisitor(ItemActionService itemActionService)
    {
        _itemActionService = itemActionService;
    }

    public Task<string> Visit(PutIndexItemActionModel model)
    {
        var item = new Item
        {
            ItemId = model.ItemId,
            GroupId = model.GroupId,
            GroupItemIds = model.GroupItemIds,
            Images = model.Images.Select(x => new Image
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Properties = model.Properties.ToExpando()
        };
        return _itemActionService.EnqueueIndexAsync(item, model.Priority);
    }

    public Task<string> Visit(PutDeleteItemActionModel model)
    {
        return _itemActionService.EnqueueDeleteItemAsync(model.ItemId);
    }

    public Task<string> Visit(PutDeleteGroupItemActionModel model)
    {
        return _itemActionService.EnqueueDeleteGroupAsync(model.GroupId);
    }
}
