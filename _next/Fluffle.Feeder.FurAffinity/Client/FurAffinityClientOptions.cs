using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.FurAffinity.Client;

internal class FurAffinityClientOptions
{
    public const string FurAffinityClient = "FurAffinityClient";

    [Required]
    public required string A { get; set; }

    [Required]
    public required string B { get; set; }

    [Required]
    public required TimeSpan RateLimitPace { get; set; }
}
