using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Main.Database;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public static class PlatformExtensions
    {
        public static Task<SR<TOut>> GetPlatformAsync<TOut>(this IQueryable<Platform> platforms,
            string platformName, Func<Platform, Task<SR<TOut>>> func)
        {
            return platforms.GetPlatformAsync(platformName, error => new SR<TOut>(error), func);
        }

        public static Task<SE> GetPlatformAsync(this IQueryable<Platform> platforms,
            string platformName, Func<Platform, Task<SE>> func)
        {
            return platforms.GetPlatformAsync(platformName, error => error, func);
        }

        public static async Task<T> GetPlatformAsync<T>(this IQueryable<Platform> platforms,
            string platformName, Func<SE, T> notFound, Func<Platform, Task<T>> func)
        {
            var platform = await platforms.FirstOrDefaultAsync(platformName);

            if (platform == null)
                return notFound(PlatformError.PlatformNotFound(platformName));

            return await func(platform);
        }
    }
}
