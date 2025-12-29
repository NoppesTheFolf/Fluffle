using System.ComponentModel.DataAnnotations;

namespace Fluffle.Ingestion.Worker.ThumbnailStorage;

public class ThumbnailStorageOptions
{
    public const string ThumbnailStorage = "ThumbnailStorage";

    [Required]
    public required string Salt { get; set; }
}
