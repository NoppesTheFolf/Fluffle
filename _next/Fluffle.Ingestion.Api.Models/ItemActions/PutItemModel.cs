using Fluffle.Ingestion.Api.Models.Items;
using System.Text.Json.Nodes;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

public class PutItemModel
{
    public required int Priority { get; set; }

    public required ICollection<ImageModel> Images { get; set; }

    public required JsonNode Properties { get; set; }
}
