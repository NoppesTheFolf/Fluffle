using System.ComponentModel.DataAnnotations;

namespace Fluffle.Content.Api.Client;

public class ContentApiClientOptions
{
    public const string ContentApiClient = "ContentApiClient";

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
