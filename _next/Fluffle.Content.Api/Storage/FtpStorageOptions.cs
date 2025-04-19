using System.ComponentModel.DataAnnotations;

namespace Fluffle.Content.Api.Storage;

public class FtpStorageOptions
{
    public const string FtpStorage = "FtpStorage";

    [Required]
    public required string Host { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string DirectoryPrefix { get; set; }

    [Required]
    public required int PoolSize { get; set; }
}
