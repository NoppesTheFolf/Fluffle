using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Framework.StatePersistence.Cosmos;

internal class CosmosOptions
{
    public const string Cosmos = "Cosmos";

    [Required]
    public required string ConnectionString { get; set; }

    public string DatabaseId { get; set; } = "fluffle";

    public string ContainerId { get; set; } = "feederStates";
}
