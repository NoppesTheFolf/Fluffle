using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(IndexItemActionModel), "index")]
[JsonDerivedType(typeof(DeleteItemActionModel), "delete")]
[JsonDerivedType(typeof(DeleteGroupItemActionModel), "deleteGroup")]
public abstract class ItemActionModel
{
    public required string ItemActionId { get; set; }

    public abstract T Visit<T>(IItemActionModelVisitor<T> visitor);
}
