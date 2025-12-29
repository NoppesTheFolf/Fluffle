using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Bluesky.JetstreamWatcher;

public class BlueskyJetstreamWatcherOptions
{
    public const string BlueskyJetstreamWatcher = "BlueskyJetstreamWatcher";

    [Required]
    public required string InstanceHostname { get; set; }
}
