using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;

namespace Fluffle.Feeder.Framework.StatePersistence.Cosmos;

internal class CosmosStateRepository<T> : IStateRepository<T>
{
    private readonly string _id;
    private readonly CosmosClientFactory _clientFactory;
    private readonly IOptions<CosmosOptions> _options;

    public CosmosStateRepository(string id, CosmosClientFactory clientFactory, IOptions<CosmosOptions> options)
    {
        _id = id;
        _clientFactory = clientFactory;
        _options = options;
    }

    public async Task PutAsync(T state)
    {
        var client = await _clientFactory.CreateAsync();
        var container = client.GetContainer(_options.Value.DatabaseId, _options.Value.ContainerId);

        await container.UpsertItemAsync(new CosmosState<T>
        {
            Id = _id,
            State = state
        }, new PartitionKey(_id));
    }

    public async Task<T?> GetAsync()
    {
        var client = await _clientFactory.CreateAsync();
        var container = client.GetContainer(_options.Value.DatabaseId, _options.Value.ContainerId);

        try
        {
            var item = await container.ReadItemAsync<CosmosState<T>>(_id, new PartitionKey(_id));

            return item.Resource.State;
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            throw;
        }
    }
}
