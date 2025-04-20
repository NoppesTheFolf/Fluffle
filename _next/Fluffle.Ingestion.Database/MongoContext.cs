using Fluffle.Ingestion.Core.Domain.ItemActions;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Fluffle.Ingestion.Database;

internal sealed class MongoContext : IDisposable
{
    private readonly MongoClient _client;

    public MongoContext(IOptions<MongoOptions> options)
    {
        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register(nameof(CamelCaseElementNameConvention), conventionPack, _ => true);

        BsonClassMap.RegisterClassMap<ItemAction>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdProperty(x => x.ItemActionId);
        });
        BsonClassMap.RegisterClassMap<IndexItemAction>();
        BsonClassMap.RegisterClassMap<DeleteItemAction>();

        _client = new MongoClient(options.Value.ConnectionString);
        var database = _client.GetDatabase(options.Value.DatabaseName);

        // Item actions
        ItemActions = database.GetCollection<ItemAction>("itemActions");

        var itemIdIndexKeys = Builders<ItemAction>.IndexKeys.Ascending(x => x.ItemId);
        ItemActions.Indexes.CreateOne(new CreateIndexModel<ItemAction>(itemIdIndexKeys, new CreateIndexOptions { Unique = true }));

        var priorityIndexKeys = Builders<ItemAction>.IndexKeys.Descending(x => x.Priority);
        ItemActions.Indexes.CreateOne(new CreateIndexModel<ItemAction>(priorityIndexKeys));

        // Item action failures
        ItemActionFailures = database.GetCollection<ItemAction>("itemActionFailures");

        ItemActionFailures.Indexes.CreateOne(new CreateIndexModel<ItemAction>(itemIdIndexKeys));
    }

    public IMongoCollection<ItemAction> ItemActions { get; }

    public IMongoCollection<ItemAction> ItemActionFailures { get; }

    public void Dispose()
    {
        _client.Dispose();
    }
}
