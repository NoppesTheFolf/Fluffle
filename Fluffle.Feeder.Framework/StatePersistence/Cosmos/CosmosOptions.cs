using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Framework.StatePersistence.Cosmos;

internal class CosmosOptions
{
    public const string Cosmos = "Cosmos";

    [Required]
    public required string ConnectionString { get; set; }

    [Required]
    public required string DatabaseId { get; set; }

    [Required]
    public required string ContainerId { get; set; }
}
