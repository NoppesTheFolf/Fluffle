using MongoDB.Driver;

namespace Noppes.Fluffle.Twitter.Database;

public class TwitterContext : BaseMongoContext
{
    public TwitterContext(string connectionString, string databaseName) : base(connectionString, databaseName)
    {
        UserImportFailures = Database.GetRepository<UserImportFailureEntity>("UserImportFailures");

        Users = Database.GetRepository<UserEntity>("Users");

        Tweets = Database.GetRepository<TweetEntity>("Tweets");
        // Make it easy to retrieve tweets from a specific user
        Tweets.Collection.Indexes.CreateOne(new CreateIndexModel<TweetEntity>(Builders<TweetEntity>.IndexKeys.Ascending(x => x.UserId)));
        // Make it easy to retrieve tweets based on the media ID field
        Tweets.Collection.Indexes.CreateOne(new CreateIndexModel<TweetEntity>(Builders<TweetEntity>.IndexKeys.Ascending("Media._id")));
    }

    public IMongoRepository<UserImportFailureEntity> UserImportFailures { get; set; }

    public IMongoRepository<UserEntity> Users { get; set; }

    public IMongoRepository<TweetEntity> Tweets { get; set; }
}
