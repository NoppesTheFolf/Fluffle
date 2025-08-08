namespace Fluffle.Ingestion.Api.Models.ItemActions;

public interface IItemActionModelVisitor<out T>
{
    T Visit(IndexItemActionModel model);

    T Visit(DeleteItemActionModel model);

    T Visit(DeleteGroupItemActionModel model);
}
