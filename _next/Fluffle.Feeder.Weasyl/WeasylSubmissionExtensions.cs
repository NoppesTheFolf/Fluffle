using Fluffle.Feeder.Weasyl.ApiClient;
using Fluffle.Ingestion.Api.Models.Items;

namespace Fluffle.Feeder.Weasyl;

internal static class WeasylSubmissionExtensions
{
    public static IEnumerable<ImageModel> GetImages(this WeasylSubmission submission)
    {
        if (submission.Media.Cover != null)
        {
            yield return new ImageModel
            {
                Url = submission.Media.Cover.Single().Url.AbsoluteUri,
                Width = 1000,
                Height = 1000
            };
        }

        if (submission.Media.Submission != null)
        {
            yield return new ImageModel
            {
                Url = submission.Media.Submission.Single().Url.AbsoluteUri,
                Width = 2000,
                Height = 2000
            };
        }
    }
}
