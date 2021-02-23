using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Noppes.Fluffle.Main.Api.Helpers
{
    public static class DataSourceExtensions
    {
        public static void SyncWithDataSources(this FluffleContext context)
        {
            var dataSourceProviders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Select(t => new
                {
                    Type = t,
                    Interfaces = t.GetInterfaces()
                        .Where(i => i.Name == typeof(IDataSource<,>).Name)
                        .Where(i => i.IsGenericType)
                        .ToList()
                })
                .Where(x => x.Interfaces.Any())
                .Select(x => new
                {
                    Instance = Activator.CreateInstance(x.Type),
                    Sources = x.Interfaces.Select(i =>
                    {
                        var genericArguments = i.GetGenericArguments();

                        return new
                        {
                            Enum = genericArguments[0],
                            Entity = genericArguments[1]
                        };
                    })
                })
                .ToList();

            foreach (var dataSourceProvider in dataSourceProviders)
            {
                foreach (var dataSource in dataSourceProvider.Sources)
                {
                    var genericGetSet = typeof(DbContext)
                        .GetMethods()
                        .Where(m => m.Name == nameof(DbContext.Set))
                        .First(m => m.IsGenericMethod)
                        .MakeGenericMethod(dataSource.Entity);

                    var set = genericGetSet.Invoke(context, null);

                    var genericSet = typeof(DbSet<>)
                        .MakeGenericType(dataSource.Entity);

                    var genericSetAdd = genericSet
                        .GetMethod(nameof(DbSet<object>.Add));

                    var getEntity = typeof(IDataSource<,>)
                        .MakeGenericType(dataSource.Enum, dataSource.Entity)
                        .GetMethod(nameof(IDataSource<HttpStatusCode, object>.From));

                    var entityProperties = dataSource.Entity.GetProperties()
                        .Where(p => p.GetAccessors().All(a => !a.IsVirtual))
                        .ToList();

                    foreach (var value in Enum.GetValues(dataSource.Enum))
                    {
                        var localEntity = getEntity.Invoke(dataSourceProvider.Instance, new[] { value });
                        ((dynamic)localEntity).Id = (int)value;

                        var dbEntity = ((IQueryable<object>)set).FirstOrDefault(e => e == localEntity);

                        if (dbEntity == null)
                        {
                            genericSetAdd.Invoke(set, new[] { localEntity });
                            continue;
                        }

                        foreach (var property in entityProperties)
                        {
                            if (property.GetCustomAttribute<SyncAttribute>() == null)
                                continue;

                            var localValue = property.GetValue(localEntity);
                            property.SetValue(dbEntity, localValue);
                        }
                    }
                }
            }
        }
    }
}
