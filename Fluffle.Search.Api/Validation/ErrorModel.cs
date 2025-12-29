namespace Fluffle.Search.Api.Validation;

public class ErrorModel
{
    public required string? Code { get; set; }

    public required string Message { get; set; }
}
