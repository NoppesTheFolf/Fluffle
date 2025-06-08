namespace Fluffle.Imaging.Api.Models;

public class ImageMetadataModel
{
    public required int Width { get; set; }

    public required int Height { get; set; }

    public required ImageMetadataCenterModel? Center { get; set; }
}
