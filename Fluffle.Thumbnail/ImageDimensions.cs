namespace Noppes.Fluffle.Thumbnail
{
    public class ImageDimensions
    {
        /// <summary>
        /// The image its width in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The image its height in pixels.
        /// </summary>
        public int Height { get; set; }

        public ImageDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
