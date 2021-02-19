using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service
{
    public abstract class Service
    {
        protected internal IServiceProvider Services { get; set; } = null!;

        protected internal abstract Task RunAsync();
    }

    public abstract class Service<TService> : Service where TService : Service
    {
        public static async Task RunAsync(Func<TService, FluffleConfiguration, IServiceCollection, Task> configureServicesAsync = null, Func<TService, Task> configureAsync = null)
        {
            var service = Activator.CreateInstance<TService>();

            var configuration = FluffleConfiguration.Load<TService>();

            var serviceCollection = new ServiceCollection();

            if (configureServicesAsync != null)
                await configureServicesAsync(service, configuration, serviceCollection);

            service.Services = serviceCollection.BuildServiceProvider();

            if (configureAsync != null)
                await configureAsync(service);

            try
            {
                await service.RunAsync();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Oh no! An non-transient exception occurred! This probably indicates there is a bug.");
            }
        }
    }
}
