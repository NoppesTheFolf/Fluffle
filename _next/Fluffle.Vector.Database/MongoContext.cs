using Fluffle.Vector.Core.Domain.Items;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Fluffle.Vector.Database;

internal sealed class MongoContext : IDisposable
{
    private readonly MongoClient _client;

    public MongoContext(IOptions<MongoOptions> options)
    {
        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register(nameof(CamelCaseElementNameConvention), conventionPack, _ => true);

        BsonClassMap.RegisterClassMap<Item>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdProperty(x => x.ItemId);
        });
        BsonClassMap.RegisterClassMap<ItemVectors>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdProperty(x => x.ItemVectorsId);
        });

        _client = new MongoClient(options.Value.ConnectionString);
        var database = _client.GetDatabase(options.Value.DatabaseName);

        Items = database.GetCollection<Item>("items");
        ItemVectors = database.GetCollection<ItemVectors>("itemVectors");
    }

    public IMongoCollection<Item> Items { get; }

    public IMongoCollection<ItemVectors> ItemVectors { get; set; }

    public void Dispose()
    {
        _client.Dispose();
    }
}
