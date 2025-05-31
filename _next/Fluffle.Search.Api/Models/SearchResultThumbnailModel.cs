namespace Fluffle.Search.Api.Models;

public class SearchResultThumbnailModel
{
    public required int Width { get; set; }

    public required int CenterX { get; set; }

    public required int Height { get; set; }

    public required int CenterY { get; set; }

    public required string Url { get; set; }
}
