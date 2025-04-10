namespace Fluffle.Ingestion.Api.Models.Items;

public class ImageModel
{
    public required int Width { get; set; }

    public required int Height { get; set; }

    public required string Url { get; set; }
}