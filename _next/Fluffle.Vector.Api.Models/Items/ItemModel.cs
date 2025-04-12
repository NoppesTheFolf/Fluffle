namespace Fluffle.Vector.Api.Models.Items;

public class ItemModel
{
    public required string ItemId { get; set; }

    public required ICollection<ImageModel> Images { get; set; }

    public required IDictionary<string, object?>? Properties { get; set; }
}
