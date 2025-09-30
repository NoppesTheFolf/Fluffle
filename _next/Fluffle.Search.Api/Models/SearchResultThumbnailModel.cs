using System.ComponentModel;

namespace Fluffle.Search.Api.Models;

public class SearchResultThumbnailModel
{
    [Description("Width of the thumbnail.")]
    public required int Width { get; set; }

    [Description("Percentage the image has to shift horizontally for its content to be centered in a square. " +
                 "Works like the object-position CSS property.")]
    public required int CenterX { get; set; }

    [Description("Height of the thumbnail.")]
    public required int Height { get; set; }

    [Description("Percentage the image has to shift vertically for its content to be centered in a square. " +
                 "Works like the object-position CSS property.")]
    public required int CenterY { get; set; }

    [Description("URL at which the thumbnail can be found.")]
    public required string Url { get; set; }
}
