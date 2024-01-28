namespace Noppes.Fluffle.Twitter.Database;

public class UserEntity
{
    public string Id { get; set; } = null!;

    public string AlternativeId { get; set; } = null!;

    public bool IsProtected { get; set; }

    public bool IsSuspended { get; set; }

    public bool IsDeleted { get; set; }

    public bool HasViolatedMediaPolicy { get; set; }

    public bool CanMediaBeRetrieved => !IsProtected && !IsSuspended && !IsDeleted & !HasViolatedMediaPolicy;

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

    // Misc metadata
    public double? TweetsPerDay { get; set; }
    public DateTime? TweetsPerDayBasedOnWhen { get; set; }
}

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
