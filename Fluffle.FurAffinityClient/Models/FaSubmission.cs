using System;

namespace Noppes.Fluffle.FurAffinity.Models;

public class FaSubmission
{
    public int Id { get; set; }

    public FaArtist Owner { get; set; }

    public string Title { get; set; }

    public FaSubmissionStats Stats { get; set; }

    public FaSubmissionRating Rating { get; set; }

    public string Species { get; set; }

    public string Gender { get; set; }

    public Uri ViewLocation { get; set; }

    public Uri FileLocation { get; set; }

    public FaSize Size { get; set; }

    public DateTimeOffset ThumbnailWhen { get; set; }

    public DateTimeOffset When { get; set; }

    public FaThumbnail GetThumbnail(int targetMax)
    {
        var thumbnail = new FaThumbnail
        {
            Location = new Uri($"https://t.furaffinity.net/{Id}@{targetMax}-{ThumbnailWhen.ToUnixTimeSeconds()}.jpg")
        };

        if (Size == null)
        {
            (thumbnail.Width, thumbnail.Height) = (-1, -1);
            return thumbnail;
        }

        static int DetermineSize(int sizeOne, int sizeTwo, int sizeOneTarget)
        {
            var aspectRatio = (double)sizeOneTarget / sizeOne;

            return (int)Math.Round(aspectRatio * sizeTwo);
        }

        if (Size.Width == Size.Height)
        {
            (thumbnail.Width, thumbnail.Height) = (targetMax, targetMax);
            return thumbnail;
        }

        (thumbnail.Width, thumbnail.Height) = Size.Width > Size.Height
            ? (DetermineSize(Size.Width, Size.Height, targetMax), targetMax)
            : (targetMax, DetermineSize(Size.Height, Size.Width, targetMax));

        return thumbnail;
    }
}
