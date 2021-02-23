using Humanizer;
using Noppes.Fluffle.Constants;
using System.Linq;

namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string Platform = "platform";
        public const string Platforms = "platforms";

        public static object[] PlatformRoute(string platformName, params object[] urlSegments) =>
            new object[] { Platform, platformName.Kebaberize() }.Concat(urlSegments).ToArray();

        public static object[] GetPlatforms =>
            V1.Url(Platforms);

        public static object[] GetPlatform(string platformName) =>
            V1.Url(PlatformRoute(platformName));

        public static object[] GetPlatformSync(string platformName) =>
            V1.Url(PlatformRoute(platformName, "sync"));

        public static object[] SignalPlatformSync(string platformName, SyncTypeConstant syncType) =>
            V1.Url(PlatformRoute(platformName, "sync", syncType));
    }
}
