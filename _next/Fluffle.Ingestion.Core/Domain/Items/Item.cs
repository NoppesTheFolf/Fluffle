namespace Fluffle.Ingestion.Core.Domain.Items;

public class Item
{
    public required string ItemId { get; set; }

    public required string? GroupId { get; set; }

    public required ICollection<string>? GroupItemIds { get; set; }

    public required ICollection<Image> Images { get; set; }

    public required object Properties { get; set; }
}
