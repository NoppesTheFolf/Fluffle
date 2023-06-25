using MongoDB.Driver;

namespace Noppes.Fluffle.Twitter.Database;

public class TwitterContext : BaseMongoContext
{
    public TwitterContext(string connectionString, string databaseName) : base(connectionString, databaseName)
    {
        UserImportFailures = GetRepository<UserImportFailureEntity>("UserImportFailures");

        // Make it easy to retrieve an import failure for a specific username
        UserImportFailures.Collection.Indexes.CreateOne(new CreateIndexModel<UserImportFailureEntity>(Builders<UserImportFailureEntity>.IndexKeys.Ascending(x => x.Username), new CreateIndexOptions
        {
            Collation = CaseInsensitiveCollation
        }));

        Users = GetRepository<UserEntity>("Users");
        // Make it easy to retrieve users with a specific username
        Users.Collection.Indexes.CreateOne(new CreateIndexModel<UserEntity>(Builders<UserEntity>.IndexKeys.Ascending(x => x.Username), new CreateIndexOptions
        {
            Collation = CaseInsensitiveCollation
        }));

        Tweets = GetRepository<TweetEntity>("Tweets");
        // Make it easy to retrieve tweets from a specific user
        Tweets.Collection.Indexes.CreateOne(new CreateIndexModel<TweetEntity>(Builders<TweetEntity>.IndexKeys.Ascending(x => x.UserId)));
        // Make it easy to retrieve tweets based on the media ID field
        Tweets.Collection.Indexes.CreateOne(new CreateIndexModel<TweetEntity>(Builders<TweetEntity>.IndexKeys.Ascending("Media._id")));
    }

    public IMongoRepository<UserImportFailureEntity> UserImportFailures { get; set; }

    public IMongoRepository<UserEntity> Users { get; set; }

    public IMongoRepository<TweetEntity> Tweets { get; set; }
}
