using Fluffle.Feeder.Bluesky.Core.Domain;
using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Fluffle.Feeder.Bluesky.Mongo;

internal sealed class MongoContext : IDisposable
{
    private readonly MongoClient _client;

    public MongoContext(IOptions<MongoOptions> options)
    {
        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register(nameof(CamelCaseElementNameConvention), conventionPack, _ => true);

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        BsonClassMap.RegisterClassMap<BlueskyCreatePostEvent>();
        BsonClassMap.RegisterClassMap<BlueskyDeletePostEvent>();
        BsonClassMap.RegisterClassMap<BlueskyDeleteAccountEvent>();

        BsonClassMap.RegisterClassMap<BlueskyProfile>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdProperty(x => x.Did);
        });

        _client = new MongoClient(options.Value.ConnectionString);
        var database = _client.GetDatabase(options.Value.DatabaseName);

        Events = database.GetCollection<BlueskyEvent>("events");
        var priorityIndexKeys = Builders<BlueskyEvent>.IndexKeys.Ascending(x => x.UnixTimeMicroseconds);
        Events.Indexes.CreateOne(new CreateIndexModel<BlueskyEvent>(priorityIndexKeys));

        Profiles = database.GetCollection<BlueskyProfile>("profiles");

        Posts = database.GetCollection<BlueskyPost>("posts");
        var didIndexKeys = Builders<BlueskyPost>.IndexKeys.Ascending(x => x.Id.Did);
        Posts.Indexes.CreateOne(new CreateIndexModel<BlueskyPost>(didIndexKeys));
    }

    public IMongoCollection<BlueskyEvent> Events { get; }

    public IMongoCollection<BlueskyProfile> Profiles { get; }

    public IMongoCollection<BlueskyPost> Posts { get; }

    public void Dispose()
    {
        _client.Dispose();
    }
}
