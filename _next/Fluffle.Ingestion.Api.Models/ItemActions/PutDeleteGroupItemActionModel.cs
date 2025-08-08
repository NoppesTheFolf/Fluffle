using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(PutDeleteGroupItemActionModel), "deleteGroup")]
public class PutDeleteGroupItemActionModel : PutItemActionModel
{
    public required string GroupId { get; set; }

    public override T Visit<T>(IPutItemActionModelVisitor<T> visitor) => visitor.Visit(this);
}
