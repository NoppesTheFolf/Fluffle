using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Noppes.Fluffle.Thumbnail;

public static class FluffleThumbnailExtensions
{
    public static void AddFluffleThumbnail(this IServiceCollection services)
    {
        services.AddSingleton<FluffleThumbnail>(_ =>
        {
            if (Debugger.IsAttached && OperatingSystem.IsWindows())
                return new SystemDrawingFluffleThumbnail();

            if (!OperatingSystem.IsLinux())
                throw new InvalidOperationException("Can't run in a non-Linux environment in production.");

            return new VipsFluffleThumbnail();
        });
    }
}
