using Humanizer;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Database.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.Database
{
    public static class PlatformError
    {
        private const string NotFoundCode = "PLATFORM_NOT_FOUND";

        public static SE PlatformNotFound(string name)
        {
            return new(NotFoundCode, HttpStatusCode.NotFound,
                $"No platform exists with name `{name}`.");
        }
    }

    public static class PlatformExtensions
    {
        public static Task<SR<TOut>> GetPlatformAsync<TPlatform, TOut>(this IQueryable<TPlatform> platforms,
            string platformName, Func<TPlatform, Task<SR<TOut>>> func) where TPlatform : IPlatform
        {
            return platforms.GetPlatformAsync(platformName, error => new SR<TOut>(error), func);
        }

        public static Task<SE> GetPlatformAsync<TPlatform>(this IQueryable<TPlatform> platforms,
            string platformName, Func<TPlatform, Task<SE>> func) where TPlatform : IPlatform
        {
            return platforms.GetPlatformAsync(platformName, error => error, func);
        }

        public static async Task<T> GetPlatformAsync<TPlatform, T>(this IQueryable<TPlatform> platforms,
            string platformName, Func<SE, T> notFound, Func<TPlatform, Task<T>> func) where TPlatform : IPlatform
        {
            platformName = platformName.Kebaberize();

            var platform = await platforms.FirstOrDefaultAsync(p => p.NormalizedName == platformName);

            if (platform == null)
                return notFound(PlatformError.PlatformNotFound(platformName));

            return await func(platform);
        }
    }
}
