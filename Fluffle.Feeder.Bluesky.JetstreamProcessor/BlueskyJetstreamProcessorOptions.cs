using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor;

public class BlueskyJetstreamProcessorOptions
{
    public const string BlueskyJetstreamProcessor = "BlueskyJetstreamProcessor";

    [Required]
    public required int WorkerCount { get; set; }

    [Required]
    public required TimeSpan? ErrorDelay { get; set; }
}
