namespace Fluffle.Imaging.Api.Client;

public class ThumbnailModel
{
    public required byte[] Thumbnail { get; set; }

    public required ImageMetadataModel Metadata { get; set; }
}
