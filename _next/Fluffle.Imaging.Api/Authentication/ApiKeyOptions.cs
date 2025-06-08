using System.ComponentModel.DataAnnotations;

namespace Fluffle.Imaging.Api.Authentication;

public class ApiKeyOptions
{
    public const string ApiKey = "ApiKey";

    [Required]
    public required string Value { get; set; }
}
