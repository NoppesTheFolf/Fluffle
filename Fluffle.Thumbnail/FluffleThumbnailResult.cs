namespace Noppes.Fluffle.Thumbnail
{
    public class FluffleThumbnailResult
    {
        /// <summary>
        /// Width of the generated thumbnail.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Offset in percentages to center the content horizontally.
        /// </summary>
        public int CenterX { get; set; }

        /// <summary>
        /// Height of the generated thumbnail.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Offset in percentages to center the content vertically.
        /// </summary>
        public int CenterY { get; set; }
    }
}
