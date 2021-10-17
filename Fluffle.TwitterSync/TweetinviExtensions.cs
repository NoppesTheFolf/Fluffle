using MoreLinq.Extensions;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.TwitterSync.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace Noppes.Fluffle.TwitterSync
{
    public static class TweetinviExtensions
    {
        public static IList<ITweet> Flatten(this IList<ITweet> tweets)
        {
            int previousCount;
            do
            {
                previousCount = tweets.Count;

                tweets = tweets
                    .SelectMany(t => new[] { t, t.RetweetedTweet, t.QuotedTweet })
                    .Where(t => t != null)
                    .DistinctBy(t => t.IdStr)
                    .ToList();
            } while (tweets.Count != previousCount);

            return tweets;
        }

        public static MediaTypeConstant MediaType(this IMediaEntity mediaEntity)
        {
            return mediaEntity.MediaType switch
            {
                "photo" => MediaTypeConstant.Image,
                "animated_gif" => MediaTypeConstant.AnimatedImage,
                "video" => MediaTypeConstant.Video,
                _ => throw new ArgumentOutOfRangeException(nameof(mediaEntity))
            };
        }

        public static ResizeMode Resize(this IMediaEntitySize size)
        {
            return size.Resize switch
            {
                "crop" => ResizeMode.Crop,
                "fit" => ResizeMode.Fit,
                _ => throw new ArgumentOutOfRangeException(nameof(size))
            };
        }

        public static MediaSizeConstant Size(this KeyValuePair<string, IMediaEntitySize> kv)
        {
            return kv.Key switch
            {
                "thumb" => MediaSizeConstant.Thumb,
                "small" => MediaSizeConstant.Small,
                "medium" => MediaSizeConstant.Medium,
                "large" => MediaSizeConstant.Large,
                _ => throw new ArgumentOutOfRangeException(nameof(kv))
            };
        }
    }
}
