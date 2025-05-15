using System.Text.Json.Nodes;

namespace Fluffle.Vector.Api.Models.Items;

public class PutItemVectorModel
{
    public required float[] Value { get; set; }

    public required JsonNode? Properties { get; set; }
}
