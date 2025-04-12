namespace Fluffle.Vector.Core.Domain.Vectors;

public class VectorSearchResult
{
    public required string ItemId { get; set; }

    public required float Distance { get; set; }
}
