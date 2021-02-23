namespace Noppes.Fluffle.Main.Communication
{
    public abstract class PutContentIndexModel
    {
        public class ThumbnailModel
        {
            public int Width { get; set; }

            public int CenterX { get; set; }

            public int Height { get; set; }

            public int CenterY { get; set; }

            public string Location { get; set; }

            public string Filename { get; set; }

            public string B2FileId { get; set; }
        }

        public ThumbnailModel Thumbnail { get; set; }
    }
}
