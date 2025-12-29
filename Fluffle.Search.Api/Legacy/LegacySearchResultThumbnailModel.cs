namespace Fluffle.Search.Api.Legacy;

public class LegacySearchResultThumbnailModel
{
    public required int Width { get; set; }

    public required int CenterX { get; set; }

    public required int Height { get; set; }

    public required int CenterY { get; set; }

    public required string Location { get; set; }
}
