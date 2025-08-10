namespace Fluffle.Search.Api.Legacy;

public class LegacyValidationError
{
    public required string Code { get; set; }

    public required string Message { get; set; }

    public required IDictionary<string, IEnumerable<string>> Errors { get; set; }
}
