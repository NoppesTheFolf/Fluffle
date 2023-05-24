using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Main.Api.Helpers;

public static class ChangeIdIncrementerExtensions
{
    private static ICollection<Type> _incrementableTypes;

    public static void RegisterChangeIdIncrementers(this IServiceCollection services)
    {
        _incrementableTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass) // Must be a class
            .Where(t => t.BaseType == typeof(BaseEntity) || t.BaseType == typeof(TrackedBaseEntity)) // Class inherits directly from (tracked) base entity
            .Where(t => typeof(ITrackable).IsAssignableFrom(t)) // Must implement ITrackable
            .Select(t => typeof(ChangeIdIncrementer<>).MakeGenericType(t))
            .ToList();

        foreach (var incrementableType in _incrementableTypes)
            services.AddSingleton(incrementableType);
    }

    public static void InitializeChangeIdIncrementers(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<FluffleContext>();

        scope.InitializeChangeIdIncrementers(context);
    }

    public static void InitializeChangeIdIncrementers(this IServiceScope scope, FluffleContext context)
    {
        foreach (var incrementer in _incrementableTypes)
        {
            var incrementers = scope.ServiceProvider.GetRequiredService(incrementer);
            ((dynamic)incrementers).Initialize(context);
        }
    }
}
