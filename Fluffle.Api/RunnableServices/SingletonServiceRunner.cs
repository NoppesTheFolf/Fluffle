using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices
{
    /// <summary>
    /// A <see cref="ServiceRunner"/> for singleton services.
    /// </summary>
    public class SingletonServiceRunner : ServiceRunner
    {
        private readonly IService _singleton;
        private bool _isInitialized;

        public SingletonServiceRunner(IServiceProvider services, Type serviceType, TimeSpan interval)
            : base(services, serviceType, interval)
        {
            _singleton = (IService)Services.GetRequiredService(serviceType);
        }

        protected override IService GetService() => _singleton;

        protected override async Task InitializeAsync(IInitializable initializable)
        {
            if (_isInitialized)
                return;

            await initializable.InitializeAsync();
            _isInitialized = true;
        }
    }
}
