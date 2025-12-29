using System.ComponentModel.DataAnnotations;

namespace Fluffle.Content.Api.Authentication;

public class ApiKeyOptions
{
    public const string ApiKey = "ApiKey";

    [Required]
    public required string Value { get; set; }
}
