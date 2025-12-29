using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.E621;

internal class E621ApiClientOptions
{
    public const string E621ApiClient = "E621ApiClient";

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string ApiKey { get; set; }

    [Required]
    public required TimeSpan RateLimitPace { get; set; }
}
