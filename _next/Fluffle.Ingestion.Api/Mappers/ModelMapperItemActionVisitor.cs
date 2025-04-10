using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Models.Items;
using Fluffle.Ingestion.Core.Domain.ItemActions;

namespace Fluffle.Ingestion.Api.Mappers;

public class ModelMapperItemActionVisitor : IItemActionVisitor<ItemActionModel>
{
    public ItemActionModel Visit(IndexItemAction itemAction)
    {
        return new IndexItemActionModel
        {
            ItemActionId = itemAction.ItemActionId!,
            Item = new ItemModel
            {
                ItemId = itemAction.Item.ItemId,
                Images = itemAction.Item.Images.Select(x => new ImageModel
                {
                    Width = x.Width,
                    Height = x.Height,
                    Url = x.Url
                }).ToList(),
                Properties = itemAction.Item.Properties
            }
        };
    }

    public ItemActionModel Visit(DeleteItemAction itemAction)
    {
        return new DeleteItemActionModel
        {
            ItemActionId = itemAction.ItemActionId!
        };
    }
}
