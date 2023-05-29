namespace Noppes.Fluffle.Client;

/// <summary>
/// Exception thrown when Fluffle's API returns an error.
/// </summary>
public class FluffleException : Exception
{
    /// <summary>
    /// The error response Fluffle's API gave.
    /// </summary>
    public FluffleErrorResponse Response { get; set; }

    /// <summary>
    /// Creates a new <see cref="FluffleException"/>.
    /// </summary>
    public FluffleException(FluffleErrorResponse response)
    {
        Response = response;
    }
}
