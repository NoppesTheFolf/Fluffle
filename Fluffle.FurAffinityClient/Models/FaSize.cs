namespace Noppes.Fluffle.FurAffinity.Models;

public class FaSize
{
    public int Width { get; set; }

    public int Height { get; set; }

    public FaSize()
    {
    }

    public FaSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
