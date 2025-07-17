using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.FurAffinity;

internal class FurAffinityFeederOptions
{
    public const string FurAffinityFeeder = "FurAffinityFeeder";

    [Required]
    public required TimeSpan NewestRunInterval { get; set; }

    [Required]
    public required TimeSpan AgedRunInterval { get; set; }

    [Required]
    public required TimeSpan MaximumAge { get; set; }
}
