using System.ComponentModel.DataAnnotations;

namespace Fluffle.Vector.Qdrant;

internal class QdrantOptions
{
    public const string Qdrant = "Qdrant";

    [Required]
    public required string Host { get; set; }

    [Required]
    public required string ApiKey { get; set; }
}
