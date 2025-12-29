using Fluffle.Feeder.Inkbunny.Client.Models;
using Fluffle.Ingestion.Api.Models.Items;

namespace Fluffle.Feeder.Inkbunny;

internal static class InkbunnySubmissionExtensions
{
    public static IEnumerable<ImageModel> GetImages(this InkbunnySubmissionFile file)
    {
        (string? url, int? width, int? height)[] fileCombinations =
        [
            (file.FullFileUrl, file.FullFileWidth, file.FullFileHeight),
            (file.NonCustomHugeThumbnailUrl, file.NonCustomHugeThumbnailWidth, file.NonCustomHugeThumbnailHeight),
            (file.NonCustomLargeThumbnailUrl, file.NonCustomLargeThumbnailWidth, file.NonCustomLargeThumbnailHeight),
            (file.NonCustomMediumThumbnailUrl, file.NonCustomMediumThumbnailWidth, file.NonCustomMediumThumbnailHeight)
        ];

        foreach (var x in fileCombinations)
        {
            if (x.url == null || x.width == null || x.height == null)
            {
                continue;
            }

            yield return new ImageModel
            {
                Url = x.url,
                Width = x.width.Value,
                Height = x.height.Value
            };
        }
    }
}
