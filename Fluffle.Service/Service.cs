using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.Configuration;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service
{
    public abstract class Service : BackgroundService
    {
        public IServiceProvider Services { get; internal set; }
        public IHostEnvironment Environment { get; }

        protected Service(IServiceProvider services)
        {
            Services = services;
            Environment = services.GetRequiredService<IHostEnvironment>();
        }
    }

    public abstract class Service<TService> : Service where TService : Service
    {
        protected Service(IServiceProvider services) : base(services)
        {
        }

        public static async Task RunAsync(string[] args, Action<FluffleConfiguration, IServiceCollection> configureServices = null)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<TService>();
                });

            hostBuilder.ConfigureServices(serviceCollection =>
            {
                var configuration = FluffleConfiguration.Load<TService>();
                serviceCollection.AddSingleton(configuration);

                configureServices?.Invoke(configuration, serviceCollection);
            }).UseSerilog();

            if (Debugger.IsAttached)
                hostBuilder.UseEnvironment("Development");

            try
            {
                await hostBuilder.Build().RunAsync();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Oh no! An non-transient exception occurred! This probably indicates there is a bug.");
            }
        }
    }
}
