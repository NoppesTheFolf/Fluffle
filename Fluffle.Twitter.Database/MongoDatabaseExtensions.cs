using MongoDB.Driver;

namespace Noppes.Fluffle.Twitter.Database;

public static class MongoDatabaseExtensions
{
    public static IMongoRepository<T> GetRepository<T>(this IMongoDatabase mongoDatabase, string name) where T : class, new()
    {
        var collection = mongoDatabase.GetCollection<T>(name);
        var repository = new MongoRepository<T>(collection);

        return repository;
    }
}