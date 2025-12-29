using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(DeleteItemActionModel), "delete")]
public class DeleteItemActionModel : ItemActionModel
{
    public required string ItemId { get; set; }

    public override T Visit<T>(IItemActionModelVisitor<T> visitor) => visitor.Visit(this);
}
