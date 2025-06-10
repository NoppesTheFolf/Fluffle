namespace Fluffle.Search.Api.Validation;

public class ErrorResponseModel
{
    public required ICollection<ErrorModel> Errors { get; set; }
}
