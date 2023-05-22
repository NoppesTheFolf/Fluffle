namespace Noppes.Fluffle.Twitter.Core;

public class ImportUserQueueItem
{
    public string Username { get; set; } = null!;

    public string Source { get; set; } = null!;
}

public class UserCheckFurryQueueItem
{
    public string Id { get; set; } = null!;
}

public class MediaIngestQueueItem
{
    /// <summary>
    /// ID of the tweet that should be processed.
    /// </summary>
    public string TweetId { get; set; } = null!;

    /// <summary>
    /// ID of the media in the tweet which should be processed.
    /// </summary>
    public string MediaId { get; set; } = null!;
}