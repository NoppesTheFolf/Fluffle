namespace Noppes.Fluffle.Client;

/// <summary>
/// Indicates how well of a match a search result is.
/// </summary>
public enum FluffleSearchMatch
{
    /// <summary>
    /// There is a very high probability of the result being an exact match.
    /// </summary>
    Exact,
    /// <summary>
    /// It can't be reliably determined whether the result is an exact match or an alternative.
    /// </summary>
    TossUp,
    /// <summary>
    /// Indicates the result is an altered version of the provided image. For example, when the
    /// submitted image displays a character with blue markings, but the result is an image of a
    /// character with yellow markings, this result in considered an alternative.
    /// </summary>
    Alternative,
    /// <summary>
    /// The chance of the result being some kind of match is very low.
    /// </summary>
    Unlikely
}
