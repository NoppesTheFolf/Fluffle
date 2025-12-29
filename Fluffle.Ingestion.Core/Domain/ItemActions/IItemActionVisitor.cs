namespace Fluffle.Ingestion.Core.Domain.ItemActions;

public interface IItemActionVisitor<out T>
{
    T Visit(IndexItemAction itemAction);

    T Visit(DeleteItemAction itemAction);

    T Visit(DeleteGroupItemAction itemAction);
}
