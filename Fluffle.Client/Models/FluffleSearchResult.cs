namespace Noppes.Fluffle.Client;

/// <summary>
/// A single search result.
/// </summary>
public class FluffleSearchResult
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A number from 0 to 1 indicating how good of a match the image is. Due to the random
    /// nature of the comparison algorithm used, this value is unlikely to drop below 0.5.
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Indicates how well of a match the search result is.
    /// </summary>
    public FluffleSearchMatch Match { get; set; }

    /// <summary>
    /// The platform (e621, Fur Affinity, etc) to which this image belongs.
    /// </summary>
    public FlufflePlatform Platform { get; set; }

    /// <summary>
    /// URL at which this image can be viewed.
    /// </summary>
    public string Location { get; set; } = null!;

    /// <summary>
    /// Whether or not this image can be considered Safe For Work.
    /// </summary>
    public bool IsSfw { get; set; }

    /// <summary>
    /// Thumbnail of the image found at <see cref="Location"/>.
    /// </summary>
    public FluffleSearchThumbnail Thumbnail { get; set; } = null!;

    /// <summary>
    /// To whom credits can be given for this image.
    /// </summary>
    public ICollection<FluffleSearchCredit> Credits { get; set; } = null!;
}
