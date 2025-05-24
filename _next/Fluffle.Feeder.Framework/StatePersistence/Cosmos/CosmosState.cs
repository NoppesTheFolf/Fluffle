namespace Fluffle.Feeder.Framework.StatePersistence.Cosmos;

internal class CosmosState<T>
{
    public required string Id { get; set; }

    public required T State { get; set; }
}
