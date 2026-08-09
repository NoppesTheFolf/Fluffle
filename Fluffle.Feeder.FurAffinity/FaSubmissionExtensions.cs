using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Feeder.FurAffinity.Client.Models;
using Fluffle.Ingestion.Api.Models.Items;

namespace Fluffle.Feeder.FurAffinity;

internal static class FaSubmissionExtensions
{
    public static IEnumerable<ImageModel> GetImages(this FaSubmission submission)
    {
        yield return new ImageModel
        {
            Url = submission.FileLocation.OriginalString,
            Width = submission.Size!.Width,
            Height = submission.Size.Height
        };

        foreach (var size in new[] { 200, 300, 400, 600 })
        {
            var thumbnail = submission.GetThumbnail(size);
            if (thumbnail == null)
            {
                continue;
            }

            yield return new ImageModel
            {
                Url = thumbnail.Location.OriginalString,
                Width = thumbnail.Width,
                Height = thumbnail.Height
            };
        }
    }

    public static string? GetDeleteReason(this FaSubmission? submission)
    {
        if (submission == null)
        {
            return "Submission was not found";
        }

        var fileExtension = Path.GetExtension(submission.FileLocation.AbsolutePath);
        if (!ImageHelper.IsSupportedExtension(fileExtension))
        {
            return $"Submission is of an unsupported type ({fileExtension})";
        }

        // Handles weird submissions like https://www.furaffinity.net/view/43164534/ where the image leads to a 404
        if (submission.Size == null)
        {
            return "Edge case where image has been deleted, but submission has not been";
        }

        return null;
    }
}
