namespace Fluffle.Ingestion.Core.Domain.ItemActions;

public class DeleteGroupItemAction : ItemAction
{
    public required string GroupId { get; set; }

    public override T Visit<T>(IItemActionVisitor<T> visitor) => visitor.Visit(this);
}
