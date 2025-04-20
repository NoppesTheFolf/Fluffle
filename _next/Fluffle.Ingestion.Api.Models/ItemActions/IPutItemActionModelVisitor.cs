namespace Fluffle.Ingestion.Api.Models.ItemActions;

public interface IPutItemActionModelVisitor<out T>
{
    T Visit(PutIndexItemActionModel model);

    T Visit(PutDeleteItemActionModel model);
}
