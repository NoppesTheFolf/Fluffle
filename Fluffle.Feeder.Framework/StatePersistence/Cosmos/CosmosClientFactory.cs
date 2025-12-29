using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;

namespace Fluffle.Feeder.Framework.StatePersistence.Cosmos;

internal class CosmosClientFactory
{
    private CosmosClient? _client;
    private readonly AsyncLock _lock = new();
    private readonly IOptions<CosmosOptions> _options;

    public CosmosClientFactory(IOptions<CosmosOptions> options)
    {
        _options = options;
    }

    public async Task<CosmosClient> CreateAsync()
    {
        using (await _lock.LockAsync())
        {
            if (_client != null)
                return _client;

            var client = new CosmosClient(_options.Value.ConnectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });

            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(_options.Value.DatabaseId, throughput: 400);
            var database = databaseResponse.Database;

            await database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = _options.Value.ContainerId,
                PartitionKeyPath = "/id",
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = false,
                    IndexingMode = IndexingMode.None
                }
            });

            _client = client;
            return _client;
        }
    }
}
