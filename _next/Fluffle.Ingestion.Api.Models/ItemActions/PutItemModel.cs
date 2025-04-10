using Fluffle.Ingestion.Api.Models.Items;
using System.Text.Json;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

public class PutItemModel
{
    public required string ItemId { get; set; }

    public required int Priority { get; set; }

    public required ICollection<ImageModel> Images { get; set; }

    public JsonElement? Properties { get; set; }
}
