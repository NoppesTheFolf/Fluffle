using System.ComponentModel;

namespace Fluffle.Search.Api.Models;

public class SearchResultModel
{
    public required string Id { get; set; }

    [Description("A number between 0 and 1 that says something about how good of a match the image is. " +
                 "The distances are only useful relative to the other images in the result. " +
                 "You should not use this for thresholds or anything like that.")]
    public required float Distance { get; set; }

    [Description("Whether the image is an exact, probable or unlikely match. " +
                 "Exact match means there is a very high probability of the result being an exact match. " +
                 "Probable match means it cannot be reliably determined whether the result is an exact match: it's either a duplicate or an alternative version of the image. " +
                 "Unlikely tells you that the chance of the result being a match is very low. " +
                 "It can however still be worth checking unlikely matches if the submitted image was cropped, for example.")]
    public required SearchResultModelMatch Match { get; set; }

    [Description("The platform (e621, Fur Affinity, etc) on which this image is hosted.")]
    public required string Platform { get; set; }

    [Description("Link to the post, submission, etc. where this image can be viewed.")]
    public required string Url { get; set; }

    [Description("Whether or not the image is considered safe for work. " +
                 "Platforms like Twitter don't disclose this. " +
                 "Images from those platforms are always marked as not safe for work.")]
    public required bool IsSfw { get; set; }

    [Description("Tiny version of the scraped image hosted by Fluffle.")]
    public required SearchResultThumbnailModel? Thumbnail { get; set; }

    [Description("To whom credits can be given for uploading this image.")]
    public required ICollection<SearchResultAuthorModel> Authors { get; set; }
}
