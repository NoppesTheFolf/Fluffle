using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;

namespace Noppes.Fluffle.FurAffinitySync
{
    public static class FurAffinityExtensions
    {
        public static IServiceCollection AddFurAffinityClient(this IServiceCollection services, FluffleConfiguration configuration, int interval, string applicationName)
        {
            var client = new FurAffinityClientFactory(configuration).CreateAsync(interval, applicationName).Result;

            return services.AddSingleton(client);
        }
    }
}
