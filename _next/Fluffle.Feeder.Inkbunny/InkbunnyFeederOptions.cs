using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Inkbunny;

internal class InkbunnyFeederOptions
{
    public const string InkbunnyFeeder = "InkbunnyFeeder";

    [Required]
    public required TimeSpan RecentRunInterval { get; set; }

    [Required]
    public required TimeSpan RecentRetrievePeriod { get; set; }

    [Required]
    public required TimeSpan ArchiveRunInterval { get; set; }
}
