using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Ingestion.Api.Models.Items;
using Noppes.E621;

namespace Fluffle.Feeder.E621;

internal static class PostExtensions
{
    public static IEnumerable<ImageModel> GetImages(this Post post)
    {
        yield return PostFileToImage(post.File!);

        if (post.Sample != null && post.Sample.Has)
        {
            yield return PostFileToImage(post.Sample);
        }

        if (post.Preview != null)
        {
            yield return PostFileToImage(post.Preview);
        }

        yield break;

        static ImageModel PostFileToImage(PostImage postImage)
        {
            return new ImageModel
            {
                Url = postImage.Location.OriginalString,
                Width = postImage.Width,
                Height = postImage.Height
            };
        }
    }

    public static ICollection<FeederAuthor> GetAuthors(this Post post)
    {
        var authors = post.Tags.Artist
            .Where(x => !x.Equals("unknown_artist", StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Equals("conditional_dnp", StringComparison.OrdinalIgnoreCase))
            .Select(x =>
            {
                var index = x.LastIndexOf("_(artist)", StringComparison.OrdinalIgnoreCase);
                var artist = index == -1 ? x : x[..index];
                artist = artist.Replace('_', ' ');

                return new FeederAuthor
                {
                    Id = x,
                    Name = artist
                };
            }).ToList();

        return authors;
    }

    public static bool IsSfw(this PostRating postRating)
    {
        var isSfw = postRating switch
        {
            PostRating.Safe => true,
            PostRating.Questionable => false,
            PostRating.Explicit => false,
            _ => throw new ArgumentOutOfRangeException(nameof(postRating), postRating, null)
        };

        return isSfw;
    }
}
