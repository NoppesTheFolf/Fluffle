﻿using Humanizer;
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

        public static object[] PlatformSyncRoute(string platformName, SyncTypeConstant syncType, params object[] urlSegments) =>
            PlatformRoute(platformName).Concat(new object[] { "sync", syncType }).Concat(urlSegments).ToArray();

        public static object[] GetPlatforms =>
            V1.Url(Platforms);

        public static object[] GetPlatform(string platformName) =>
            V1.Url(PlatformRoute(platformName));

        public static object[] GetPlatformSync(string platformName, SyncTypeConstant syncType) =>
            V1.Url(PlatformSyncRoute(platformName, syncType));

        public static object[] SignalPlatformSync(string platformName, SyncTypeConstant syncType) =>
            V1.Url(PlatformSyncRoute(platformName, syncType));

        public static object[] SyncState(string platformName) =>
            V1.Url(PlatformRoute(platformName, "sync-state"));
    }
}
