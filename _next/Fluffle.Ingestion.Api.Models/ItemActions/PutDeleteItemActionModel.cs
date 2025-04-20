using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(PutDeleteItemActionModel), "delete")]
public class PutDeleteItemActionModel : PutItemActionModel
{
    public override T Visit<T>(IPutItemActionModelVisitor<T> visitor) => visitor.Visit(this);
}
