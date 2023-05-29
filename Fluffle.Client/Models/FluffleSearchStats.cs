namespace Noppes.Fluffle.Client;

/// <summary>
/// Statistics about a request.
/// </summary>
public class FluffleSearchStats
{
    /// <summary>
    /// The number of images that got compared.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The amount of time in milliseconds it took the server to process your request.
    /// </summary>
    public float ElapsedMilliseconds { get; set; }
}
