using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(PutIndexItemActionModel), "index")]
[JsonDerivedType(typeof(PutDeleteItemActionModel), "delete")]
[JsonDerivedType(typeof(PutDeleteGroupItemActionModel), "deleteGroup")]
public abstract class PutItemActionModel
{
    public abstract T Visit<T>(IPutItemActionModelVisitor<T> visitor);
}
