using System.Linq;

namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string Image = "image";
        public const string Images = "images";

        public static object[] ImagesRoute(string platformName, params object[] urlSegments) =>
            PlatformRoute(platformName).Concat(new object[] { Images }).Concat(urlSegments).ToArray();

        public static object[] ImageRoute(string platformName, string platformImageId, params object[] urlSegments) =>
            PlatformRoute(platformName).Concat(new object[] { Image, platformImageId }).Concat(urlSegments).ToArray();

        public static object[] GetUnprocessedImages(string platformName) =>
            V1.Url(ImagesRoute(platformName, "unprocessed"));

        public static object[] PutImageIndex(string platformName, string platformImageId) =>
            V1.Url(ImageRoute(platformName, platformImageId, "index"));
    }
}
