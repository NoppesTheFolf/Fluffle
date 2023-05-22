using System.Globalization;

namespace Noppes.Fluffle.Twitter.Client;

public class TwitterUserModel
{
    public string Id { get; set; } = null!;

    public string RestId { get; set; } = null!;

    public bool IsProtected { get; set; }

    public string CreatedAt { get; set; } = null!;

    public DateTime CreatedAtParsed => DateTime.ParseExact(CreatedAt, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

    public bool DefaultProfile { get; set; }

    public bool DefaultProfileImage { get; set; }

    public string Description { get; set; } = null!;

    public int FollowersCount { get; set; }

    public int FollowingCount { get; set; }

    public string Name { get; set; } = null!;

    public string ProfileBannerUrl { get; set; } = null!;

    public string ProfileImageUrl { get; set; } = null!;

    public string Username { get; set; } = null!;
}

public enum TwitterUserError
{
    NotFound = 1,
    Suspended = 2
}

public class TwitterUserErrorModel
{
    public string Message { get; set; } = null!;

    public TwitterUserError Reason { get; set; }
}

public class TwitterUserException : Exception
{
    public TwitterUserErrorModel Error { get; set; }

    public TwitterUserException(TwitterUserErrorModel error)
    {
        Error = error;
    }
}
