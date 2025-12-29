using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.Framework.StatePersistence.Cosmos;

internal class CosmosStateRepositoryFactory : IStateRepositoryFactory
{
    private readonly CosmosClientFactory _clientFactory;
    private readonly IOptions<CosmosOptions> _options;

    public CosmosStateRepositoryFactory(CosmosClientFactory clientFactory, IOptions<CosmosOptions> options)
    {
        _clientFactory = clientFactory;
        _options = options;
    }

    public IStateRepository<T> Create<T>(string id)
    {
        return new CosmosStateRepository<T>(id, _clientFactory, _options);
    }
}
