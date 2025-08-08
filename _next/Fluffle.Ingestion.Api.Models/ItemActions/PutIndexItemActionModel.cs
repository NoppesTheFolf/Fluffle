using Fluffle.Ingestion.Api.Models.Items;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(PutIndexItemActionModel), "index")]
public class PutIndexItemActionModel : PutItemActionModel
{
    public required string ItemId { get; set; }

    public required long Priority { get; set; }

    public required string? GroupId { get; set; }

    public required ICollection<string>? GroupItemIds { get; set; }

    public required ICollection<ImageModel> Images { get; set; }

    public required JsonNode Properties { get; set; }

    public override T Visit<T>(IPutItemActionModelVisitor<T> visitor) => visitor.Visit(this);
}
