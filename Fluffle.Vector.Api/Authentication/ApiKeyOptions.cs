using System.ComponentModel.DataAnnotations;

namespace Fluffle.Vector.Api.Authentication;

public class ApiKeyOptions
{
    public const string ApiKey = "ApiKey";

    [Required]
    public required string Value { get; set; }
}
