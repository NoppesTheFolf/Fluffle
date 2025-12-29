using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Feeder.Framework.Ingestion;

public class PutDeleteItemActionModelBuilder
{
    private string? _itemId;

    public PutDeleteItemActionModelBuilder WithItemId(string itemId)
    {
        _itemId = itemId;

        return this;
    }

    public PutDeleteItemActionModel Build()
    {
        if (string.IsNullOrWhiteSpace(_itemId)) throw new InvalidOperationException("Item ID has not been set.");

        return new PutDeleteItemActionModel
        {
            ItemId = _itemId
        };
    }
}
