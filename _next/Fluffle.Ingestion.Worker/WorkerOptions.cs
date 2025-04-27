using System.ComponentModel.DataAnnotations;

namespace Fluffle.Ingestion.Worker;

public class WorkerOptions
{
    public const string Worker = "Worker";

    [Required]
    public required int WorkerCount { get; set; }

    [Required]
    public required TimeSpan DequeueInterval { get; set; }
}
