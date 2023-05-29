namespace Noppes.Fluffle.Client;

/// <summary>
/// An image thumbnail.
/// </summary>
public class FluffleSearchThumbnail
{
    /// <summary>
    /// Width of the thumbnail.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Percentage the image has to shift horizontally for its content to be centered in a
    /// square. Works like the object-position CSS property.
    /// </summary>
    public int CenterX { get; set; }

    /// <summary>
    /// Height of the thumbnail.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Percentage the image has to shift vertically for its content to be centered in a square.
    /// Works like the object-position CSS property.
    /// </summary>
    public int CenterY { get; set; }

    /// <summary>
    /// URL at which the thumbnail can be found.
    /// </summary>
    public string Location { get; set; } = null!;
}
