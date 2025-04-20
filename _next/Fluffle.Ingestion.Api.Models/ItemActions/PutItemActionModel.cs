using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(PutIndexItemActionModel), "index")]
[JsonDerivedType(typeof(PutDeleteItemActionModel), "delete")]
public abstract class PutItemActionModel
{
    public required string ItemId { get; set; }

    public abstract T Visit<T>(IPutItemActionModelVisitor<T> visitor);
}
