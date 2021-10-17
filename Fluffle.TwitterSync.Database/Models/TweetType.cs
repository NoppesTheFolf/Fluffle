using Tweetinvi.Models;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public enum TweetType
    {
        Post = 1,
        Reply = 2,
        Retweet = 3,
        QuoteTweet = 4
    }

    public static class TweetTypeExtensions
    {
        public static TweetType Type(this ITweet tweet)
        {
            if (tweet.QuotedStatusIdStr != null)
                return TweetType.QuoteTweet;

            if (tweet.IsRetweet)
                return TweetType.Retweet;

            if (tweet.InReplyToStatusIdStr != null)
                return TweetType.Reply;

            return TweetType.Post;
        }
    }
}
