using System.ComponentModel.DataAnnotations;

namespace Fluffle.Inference.Api.Client;

internal class InferenceApiClientOptions
{
    public const string InferenceApiClient = "InferenceApiClient";

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
