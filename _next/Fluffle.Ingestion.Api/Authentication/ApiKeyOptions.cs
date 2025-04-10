using System.ComponentModel.DataAnnotations;

namespace Fluffle.Ingestion.Api.Authentication;

public class ApiKeyOptions
{
    public const string ApiKey = "ApiKey";

    [Required]
    public required string Value { get; set; }
}
