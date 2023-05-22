using MongoDB.Driver;

namespace Noppes.Fluffle.Twitter.Database;

public class UserFurryPredictionEntity
{
    public int Version { get; set; }

    public bool Value { get; set; }

    public double Score { get; set; }

    public DateTime DeterminedWhen { get; set; }

    public IList<UserFurryPredictionImageUsedEntity> ImagesUsed { get; set; } = null!;
}

public class UserFurryPredictionImageUsedEntity
{
    public string MediaId { get; set; } = null!;

    public string Url { get; set; } = null!;

    public double Score { get; set; }
}

public class UserEntity
{
    public string Id { get; set; } = null!;

    public string AlternativeId { get; set; } = null!;

    public bool IsProtected { get; set; }

    public bool IsSuspended { get; set; }

    public bool IsDeleted { get; set; }

    public bool CanMediaBeRetrieved => !IsProtected && !IsSuspended && !IsDeleted;

    public DateTime CreatedAt { set; get; }

    public string? Description { get; set; }

    /// <summary>
    /// The number of users that follow the user.
    /// </summary>
    public int FollowersCount { get; set; }

    /// <summary>
    /// The number of users the user follows.
    /// </summary>
    public int FollowingCount { get; set; }

    public string Name { get; set; } = null!;

    public string? ProfileBannerUrl { get; set; }

    public string ProfileImageUrl { get; set; } = null!;

    public string Username { get; set; } = null!;

    // Metadata about whether the user is a furry artist or not
    public UserFurryPredictionEntity? FurryPrediction { get; set; }

    // Metadata about importing
    public DateTime ImportedAt { get; set; }

    // Metadata about media scraping
    public DateTime? MediaScrapingLastStartedAt { get; set; }
    public DateTime? MediaLastScrapedAt { get; set; }
}

public class TweetEntity
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int LikeCount { get; set; }

    public int QuoteCount { get; set; }

    public int ReplyCount { get; set; }

    public int RetweetCount { get; set; }

    public int BookmarkCount { get; set; }

    public IList<TweetMediaEntity> Media { get; set; } = null!;

    public DateTime CreatedAt { set; get; }
}

public class TweetMediaEntity
{
    public string Id { get; set; } = null!;

    public string Type { get; set; } = null!;

    public IList<TweetMediaPhotoEntity>? Photos { get; set; }

    public TweetMediaVideoEntity? Video { get; set; }

    public TweetMediaFurryPredictionEntity? FurryPrediction { get; set; }
}

public class TweetMediaFurryPredictionEntity
{
    public int Version { get; set; }

    public bool Value { get; set; }

    public double Score { get; set; }

    public DateTime DeterminedWhen { get; set; }
}

public class TweetMediaVideoVariantEntity
{
    public int? Bitrate { get; set; }

    public string ContentType { get; set; } = null!;

    public string Url { get; set; } = null!;
}

public class TweetMediaVideoEntity
{
    public int? Duration { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public IList<TweetMediaVideoVariantEntity> Variants { get; set; } = null!;
}

public class TweetMediaPhotoEntity
{
    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int Width { get; set; }

    public int Height { get; set; }
}

public class UserImportFailureEntity
{
    /// <summary>
    /// Username of the user for which the import failed.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Why the import failed.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// When that 
    /// </summary>
    public DateTime ImportedAt { get; set; }
}

public class TwitterContext
{
    public TwitterContext(string connectionString, string database)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        // Can be uncommented for some very nice logging of commands...
        /*settings.ClusterConfigurator = options =>
        {
            options.Subscribe<CommandStartedEvent>(e =>
            {
                Console.WriteLine();
                Console.WriteLine(e.Command.ToString());
                Console.WriteLine();
            });
        };*/

        var mongoClient = new MongoClient(settings);
        var mongoDatabase = mongoClient.GetDatabase(database);

        UserImportFailures = mongoDatabase.GetRepository<UserImportFailureEntity>("UserImportFailures");

        Users = mongoDatabase.GetRepository<UserEntity>("Users");

        Tweets = mongoDatabase.GetRepository<TweetEntity>("Tweets");
        // Make it easy to retrieve tweets based on the media ID field
        Tweets.Collection.Indexes.CreateOne(new CreateIndexModel<TweetEntity>(Builders<TweetEntity>.IndexKeys.Ascending("Media._id")));
    }

    public IMongoRepository<UserImportFailureEntity> UserImportFailures { get; set; }

    public IMongoRepository<UserEntity> Users { get; set; }

    public IMongoRepository<TweetEntity> Tweets { get; set; }
}
