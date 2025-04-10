namespace Fluffle.Ingestion.Core.Domain.Items;

public class Image
{
    public required int Width { get; set; }

    public required int Height { get; set; }

    public required string Url { get; set; }
}