namespace Fluffle.Feeder.Bluesky.Core.Domain;

public class BlueskyPostId
{
    public string Did { get; set; }

    public string RKey { get; set; }

    public BlueskyPostId(string did, string rKey)
    {
        Did = did;
        RKey = rKey;
    }
}
