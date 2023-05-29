namespace Noppes.Fluffle.Client;

/// <summary>
/// The response sent by Fluffle when an error occurs.
/// </summary>
public class FluffleErrorResponse
{
    /// <summary>
    /// Unique code that relates to the specific reason the request failed.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Message for developers detailing what went wrong.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// A list of error messages per field.
    /// </summary>
    public IDictionary<string, IList<string>>? Errors { get; set; }
}
