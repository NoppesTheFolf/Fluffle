using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Legacy;

public class LegacyFeederOptions
{
    public const string LegacyFeeder = "LegacyFeeder";

    [Required]
    public required TimeSpan RunInterval { get; set; }

    public class Platform
    {
        [Required]
        public required string Id { get; set; }

        [Required]
        public required string Prefix { get; set; }
    }

    [Required]
    public required IList<Platform> Platforms { get; set; }
}
