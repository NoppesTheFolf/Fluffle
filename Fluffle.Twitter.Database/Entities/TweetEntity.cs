namespace Noppes.Fluffle.Twitter.Database;

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

public class TweetMediaPhotoEntity
{
    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int Width { get; set; }

    public int Height { get; set; }
}

public class TweetMediaFurryPredictionEntity
{
    public int Version { get; set; }

    public bool Value { get; set; }

    public double Score { get; set; }

    public DateTime DeterminedWhen { get; set; }
}

public class TweetMediaVideoEntity
{
    public int? Duration { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public IList<TweetMediaVideoVariantEntity> Variants { get; set; } = null!;
}

public class TweetMediaVideoVariantEntity
{
    public int? Bitrate { get; set; }

    public string ContentType { get; set; } = null!;

    public string Url { get; set; } = null!;
}
