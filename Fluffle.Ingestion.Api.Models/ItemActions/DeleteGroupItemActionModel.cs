using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(DeleteGroupItemActionModel), "deleteGroup")]
public class DeleteGroupItemActionModel : ItemActionModel
{
    public required string GroupId { get; set; }

    public override T Visit<T>(IItemActionModelVisitor<T> visitor) => visitor.Visit(this);
}
