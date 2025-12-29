using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Weasyl.ApiClient;

internal class WeasylApiClientOptions
{
    public const string WeasylApiClient = "WeasylApiClient";

    [Required]
    public required string ApiKey { get; set; }

    [Required]
    public required TimeSpan RateLimitPace { get; set; }
}
