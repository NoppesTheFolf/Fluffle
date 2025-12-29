using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Inkbunny.Client;

internal class InkbunnyClientOptions
{
    public const string InkbunnyClient = "InkbunnyClient";

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required TimeSpan RateLimitPace { get; set; }
}
