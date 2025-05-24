namespace Fluffle.Feeder.Legacy.MainApi;

public class ImageModel
{
    public int Id { get; set; }

    public int PlatformId { get; set; }

    public string IdOnPlatform { get; set; } = null!;

    public long ChangeId { get; set; }

    public string ViewLocation { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public bool IsSfw { get; set; }

    public class ThumbnailModel
    {
        public int Width { get; set; }

        public int CenterX { get; set; }

        public int Height { get; set; }

        public int CenterY { get; set; }

        public string Location { get; set; } = null!;
    }

    public ThumbnailModel? Thumbnail { get; set; }

    public ICollection<int> Credits { get; set; } = null!;
}
