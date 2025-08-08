using Fluffle.Ingestion.Core.Domain.Items;

namespace Fluffle.Ingestion.Core.Domain.ItemActions;

public class IndexItemAction : ItemAction
{
    public required string ItemId { get; set; }

    public required string? GroupId { get; set; }

    public required Item Item { get; set; }

    public override T Visit<T>(IItemActionVisitor<T> visitor) => visitor.Visit(this);
}
