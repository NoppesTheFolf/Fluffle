using System.Globalization;

namespace Noppes.Fluffle.Twitter.Client;

public class TwitterTweetMediaVideoVariantModel
{
    public int? Bitrate { get; set; }

    public string ContentType { get; set; } = null!;

    public string Url { get; set; } = null!;
}

public class TwitterTweetMediaVideoModel
{
    public int? Duration { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public IList<TwitterTweetMediaVideoVariantModel> Variants { get; set; } = null!;
}

public class TwitterTweetMediaPhotoModel
{
    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int Width { get; set; }

    public int Height { get; set; }
}

public class TwitterTweetMediaModel
{
    public string Id { get; set; } = null!;

    public string Type { get; set; } = null!;

    public IList<TwitterTweetMediaPhotoModel>? Photos { get; set; }

    public TwitterTweetMediaVideoModel? Video { get; set; }
}

public class TwitterTweetModel
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int FavoriteCount { get; set; }

    public int QuoteCount { get; set; }

    public int RetweetCount { get; set; }

    public int BookmarkCount { get; set; }

    public int ReplyCount { get; set; }

    public IList<TwitterTweetMediaModel> Media { get; set; } = null!;

    public string CreatedAt { get; set; } = null!;

    public DateTime CreatedAtParsed => DateTime.ParseExact(CreatedAt, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
}

public class TwitterGetMediaResponseModel
{
    public string? Next { get; set; }

    public IList<TwitterTweetModel> Tweets { get; set; } = null!;
}
