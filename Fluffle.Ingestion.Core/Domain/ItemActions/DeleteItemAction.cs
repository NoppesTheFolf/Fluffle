namespace Fluffle.Ingestion.Core.Domain.ItemActions;

public class DeleteItemAction : ItemAction
{
    public required string ItemId { get; set; }

    public override T Visit<T>(IItemActionVisitor<T> visitor) => visitor.Visit(this);
}
