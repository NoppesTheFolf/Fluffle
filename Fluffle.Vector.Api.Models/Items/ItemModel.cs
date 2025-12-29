using System.Text.Json.Nodes;

namespace Fluffle.Vector.Api.Models.Items;

public class ItemModel
{
    public required string ItemId { get; set; }

    public required string? GroupId { get; set; }

    public required ICollection<ImageModel> Images { get; set; }

    public required ThumbnailModel? Thumbnail { get; set; }

    public required JsonNode Properties { get; set; }
}
