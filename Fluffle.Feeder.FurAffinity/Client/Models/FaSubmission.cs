namespace Fluffle.Feeder.FurAffinity.Client.Models;

internal class FaSubmission
{
    public required int Id { get; set; }

    public required Uri ViewLocation { get; set; }

    public required FaOwner Owner { get; set; }

    public required FaSubmissionRating Rating { get; set; }

    public required Uri FileLocation { get; set; }

    public required FaSize? Size { get; set; }

    public required DateTimeOffset ThumbnailWhen { get; set; }

    public required DateTimeOffset When { get; set; }

    public FaThumbnail? GetThumbnail(int target)
    {
        if (Size == null)
        {
            throw new InvalidOperationException("Cannot get a thumbnail for a submission without a size.");
        }

        var scalingFactor = target / (double)Math.Max(Size.Width, Size.Height);
        var width = (int)Math.Round(scalingFactor * Size.Width);
        var height = (int)Math.Round(scalingFactor * Size.Height);

        if (width > Size.Width || height > Size.Height)
        {
            return null;
        }

        var thumbnail = new FaThumbnail
        {
            Location = new Uri($"https://t.furaffinity.net/{Id}@{target}-{ThumbnailWhen.ToUnixTimeSeconds()}.jpg"),
            Width = width,
            Height = height
        };
        return thumbnail;
    }
}
