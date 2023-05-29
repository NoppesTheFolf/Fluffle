namespace Noppes.Fluffle.Client;

/// <summary>
/// The response from Fluffle when reverse searching.
/// </summary>
public class FluffleSearchResponse
{
    /// <summary>
    /// The unique ID of your request. If createLink was set to true, then this is the ID
    /// pointing to where your search results can be found, at: https://fluffle.xyz/q/{id}.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Statistics about the request.
    /// </summary>
    public FluffleSearchStats Stats { get; set; } = null!;

    /// <summary>
    /// The best matching images, ordered by how good the match is.
    /// </summary>
    public IList<FluffleSearchResult> Results { get; set; } = null!;
}
