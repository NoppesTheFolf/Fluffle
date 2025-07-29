using System.Text.Json.Nodes;

namespace Fluffle.Ingestion.Api.Models.Items;

public class ItemModel
{
    public required string ItemId { get; set; }

    public required string? GroupId { get; set; }

    public required ICollection<string>? GroupItemIds { get; set; }

    public required ICollection<ImageModel> Images { get; set; }

    public required JsonNode Properties { get; set; }
}
