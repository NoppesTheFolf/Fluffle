using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;

namespace Noppes.Fluffle.Twitter.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTwitterApiClient(this IServiceCollection services, FluffleConfiguration conf)
    {
        var twitterApiConf = conf.Get<TwitterApiConfiguration>();
        services.AddSingleton<ITwitterApiClient>(new TwitterApiClient(twitterApiConf.Url, twitterApiConf.ApiKey));

        return services;
    }
}
