using System.ComponentModel.DataAnnotations;

namespace Fluffle.Ingestion.Api.Client;

internal class IngestionApiClientOptions
{
    public const string IngestionApiClient = "IngestionApiClient";

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
