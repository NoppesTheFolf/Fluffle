using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Legacy.MainApi;

public class MainApiClientOptions
{
    public const string MainApiClient = "MainApiClient";

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
