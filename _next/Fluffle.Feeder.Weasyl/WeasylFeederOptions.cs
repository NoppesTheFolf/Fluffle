using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Weasyl;

internal class WeasylFeederOptions
{
    public const string WeasylFeeder = "WeasylFeeder";

    [Required]
    public required TimeSpan NewestRunInterval { get; set; }
}
