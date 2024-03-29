﻿namespace Noppes.Fluffle.Thumbnail;

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

    /// <summary>
    /// The image its area (width * height).
    /// </summary>
    public int Area
    {
        get
        {
            checked
            {
                return Width * Height;
            }
        }
    }

    public ImageDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
