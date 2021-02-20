using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Noppes.Fluffle.Api.Services
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers all non-abstract classes which implement the <see cref="Service"/> class as
        /// scoped services.
        /// </summary>
        public static void AddServices(this IServiceCollection services)
        {
            var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => typeof(Service).IsAssignableFrom(t));

            foreach (var serviceType in serviceTypes)
            {
                var serviceInterfaces = serviceType.GetInterfaces();

                if (serviceInterfaces.Length == 0)
                    throw new InvalidOperationException($"Service of type `{serviceType.Name}` doesn't implement any interface.");

                if (serviceInterfaces.Length > 1)
                    throw new InvalidOperationException($"Service of type `{serviceType.Name}` implements multiple interfaces.");

                var serviceInterface = serviceInterfaces.First();

                services.AddScoped(serviceInterface, serviceType);
            }
        }
    }
}
