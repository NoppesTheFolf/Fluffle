using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Models.Items;
using Fluffle.Ingestion.Core.Domain.ItemActions;
using System.Text.Json;

namespace Fluffle.Ingestion.Api.Visitors;

public class ModelMapperItemActionVisitor : IItemActionVisitor<ItemActionModel>
{
    public ItemActionModel Visit(IndexItemAction itemAction)
    {
        return new IndexItemActionModel
        {
            ItemActionId = itemAction.ItemActionId!,
            ItemId = itemAction.ItemId,
            Item = new ItemModel
            {
                ItemId = itemAction.Item.ItemId,
                Images = itemAction.Item.Images.Select(x => new ImageModel
                {
                    Width = x.Width,
                    Height = x.Height,
                    Url = x.Url
                }).ToList(),
                Properties = JsonSerializer.SerializeToNode(itemAction.Item.Properties) ??
                             throw new InvalidOperationException("Item properties should never serialize to null.")
            }
        };
    }

    public ItemActionModel Visit(DeleteItemAction itemAction)
    {
        return new DeleteItemActionModel
        {
            ItemActionId = itemAction.ItemActionId!,
            ItemId = itemAction.ItemId
        };
    }
}
