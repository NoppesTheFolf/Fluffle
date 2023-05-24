namespace Noppes.Fluffle.TwitterSync.Database.Models;

public class TweetMedia
{
    public string TweetId { get; set; }
    public virtual Tweet Tweet { get; set; }

    public string MediaId { get; set; }
    public virtual Media Media { get; set; }
}
