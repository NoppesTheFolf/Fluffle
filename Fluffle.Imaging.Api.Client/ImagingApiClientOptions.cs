using System.ComponentModel.DataAnnotations;

namespace Fluffle.Imaging.Api.Client;

internal class ImagingApiClientOptions
{
    public const string ImagingApiClient = "ImagingApiClient";

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
