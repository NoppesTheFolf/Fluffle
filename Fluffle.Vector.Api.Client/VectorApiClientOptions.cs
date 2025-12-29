using System.ComponentModel.DataAnnotations;

namespace Fluffle.Vector.Api.Client;

internal class VectorApiClientOptions
{
    public const string VectorApiClient = "VectorApiClient";

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
