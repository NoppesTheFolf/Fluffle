using System.ComponentModel.DataAnnotations;

namespace Fluffle.Ingestion.Worker.ThumbnailStorage;

public class FtpThumbnailStorageOptions
{
    public const string FtpThumbnailStorage = "FtpThumbnailStorage";

    [Required]
    public required string Host { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string Directory { get; set; }

    [Required]
    public required string Salt { get; set; }
}
