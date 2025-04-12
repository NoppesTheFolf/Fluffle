namespace Fluffle.Vector.Core.Domain.Items;

public class Item
{
    public required string ItemId { get; set; }

    public required ICollection<Image> Images { get; set; }

    public required IDictionary<string, object?>? Properties { get; set; }
}
