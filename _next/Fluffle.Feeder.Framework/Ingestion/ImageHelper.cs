namespace Fluffle.Feeder.Framework.Ingestion;

public static class ImageHelper
{
    private static readonly HashSet<string> SupportedExtensions =
        new(["jpg", "jpeg", "png", "webp", "gif"], StringComparer.OrdinalIgnoreCase);

    public static bool IsSupportedExtension(string extension)
    {
        extension = extension.Trim();

        if (extension.StartsWith('.'))
            extension = extension[1..];

        var isSupported = SupportedExtensions.Contains(extension);
        return isSupported;
    }
}
