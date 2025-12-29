using Fluffle.Feeder.Weasyl.ApiClient;
using Fluffle.Ingestion.Api.Models.Items;

namespace Fluffle.Feeder.Weasyl;

internal static class WeasylSubmissionExtensions
{
    public static IEnumerable<ImageModel> GetImages(this WeasylSubmission submission)
    {
        // The weird checking below has to do with https://www.weasyl.com/~kashmere/submissions/318979/husky-tough
        // That submissions contains more than a single image in the cover/submission fields, but refer to the same file
        if (submission.Media.Cover != null)
        {
            var uniqueCoversCount = submission.Media.Cover.Select(x => x.Url.AbsoluteUri).Distinct().Count();
            if (uniqueCoversCount > 1)
            {
                throw new InvalidOperationException($"Submission with ID {submission.SubmitId} contains more than a single cover media.");
            }

            yield return new ImageModel
            {
                Url = submission.Media.Cover.First().Url.AbsoluteUri,
                Width = 1000,
                Height = 1000
            };
        }

        if (submission.Media.Submission != null)
        {
            var uniqueSubmissionsCount = submission.Media.Submission.Select(x => x.Url.AbsoluteUri).Distinct().Count();
            if (uniqueSubmissionsCount > 1)
            {
                throw new InvalidOperationException($"Submission with ID {submission.SubmitId} contains more than a single submission media.");
            }

            yield return new ImageModel
            {
                Url = submission.Media.Submission.First().Url.AbsoluteUri,
                Width = 2000,
                Height = 2000
            };
        }
    }
}
