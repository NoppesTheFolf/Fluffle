namespace Fluffle.Ingestion.Core.Domain.ItemActions;

public class DeleteItemAction : ItemAction
{
    public override T Visit<T>(IItemActionVisitor<T> visitor) => visitor.Visit(this);
}
