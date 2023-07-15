namespace Noppes.Fluffle.Imaging.Tests;

internal static class Directories
{
    private const string RootDir = "./ImagingTests";
    
    public static readonly string SourceImagesDir = Path.Join(RootDir, "SourceImages");
    public static readonly string ThumbnailDestDir = Path.Join(RootDir, "ThumbnailedImages");
    public static readonly string PreconvertedImagesDir = Path.Join(RootDir, "PreconvertedImages");
    public static readonly string PreconvertedExpectedDir = Path.Join(RootDir, "PreconvertedExpected");
}