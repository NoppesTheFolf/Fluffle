namespace Noppes.Fluffle.Client;

/// <summary>
/// An entity to which credits can be given.
/// </summary>
public class FluffleSearchCredit
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The interpretation of this field is somewhat dependent on the platform from which the
    /// image was scraped. For e621, it's based on the artist tags and it's therefore safe to
    /// assume this field includes the names of the artist(s) that created the artwork. For all
    /// other platforms, it's the name of user that uploaded said image, which might be the
    /// artist, a commissioner, etc.
    /// </summary>
    public string Name { get; set; } = null!;
}
