using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.E621;

public class E621FeederOptions
{
    public const string E621Feeder = "E621Feeder";

    [Required]
    public required TimeSpan RecentRunInterval { get; set; }

    [Required]
    public required TimeSpan RecentRetrievePeriod { get; set; }

    [Required]
    public required TimeSpan CompleteRunInterval { get; set; }
}
