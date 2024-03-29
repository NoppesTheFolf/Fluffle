﻿using System.Linq;

namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string Content = "content";

        public static object[] ContentRoute(string platformName, params object[] urlSegments) =>
            PlatformRoute(platformName).Concat(new object[] { Content }).Concat(urlSegments).ToArray();

        public static object[] ContentRoute(string platformName, string platformContentId, params object[] urlSegments) =>
            PlatformRoute(platformName).Concat(new object[] { Content, platformContentId }).Concat(urlSegments).ToArray();

        public static object[] SearchContent(string platformName) =>
            V1.Url(ContentRoute(platformName, "search"));

        public static object[] DeleteContentRange(string platformName) =>
            V1.Url(ContentRoute(platformName, "range"));

        public static object[] DeleteContent(string platformName) =>
            V1.Url(ContentRoute(platformName));

        public static object[] PutContent(string platformName) =>
            V1.Url(ContentRoute(platformName));

        public static object[] GetContentToRetry(string platformName) =>
            V1.Url(ContentRoute(platformName, "retry"));

        public static object[] PutContentWarning(string platformName, string platformContentId) =>
            V1.Url(ContentRoute(platformName, platformContentId, "warning"));

        public static object[] PutContentError(string platformName, string platformContentId) =>
            V1.Url(ContentRoute(platformName, platformContentId, "error"));

        public static object[] GetMinId(string platformName) =>
            V1.Url(ContentRoute(platformName, "min-id"));

        public static object[] GetMaxId(string platformName) =>
            V1.Url(ContentRoute(platformName, "max-id"));
    }
}
