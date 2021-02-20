using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices
{
    /// <summary>
    /// A <see cref="ServiceRunner"/> for transient services.
    /// </summary>
    public class TransientServiceRunner : ServiceRunner
    {
        public TransientServiceRunner(IServiceProvider services, Type serviceType, TimeSpan interval) : base(services, serviceType, interval)
        {
        }

        protected override IService GetService() => (IService)Services.GetRequiredService(ServiceType);

        protected override Task InitializeAsync(IInitializable initializable) => initializable.InitializeAsync();
    }
}
