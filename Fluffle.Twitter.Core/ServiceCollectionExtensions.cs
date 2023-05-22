using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Queue.Queuey;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Core.Services;
using Noppes.Fluffle.Twitter.Database;

namespace Noppes.Fluffle.Twitter.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, FluffleConfiguration conf)
    {
        var syncConf = conf.Get<TwitterSyncConfiguration>();
        services.AddSingleton(syncConf);

        // Machine learning API client
        var mlApiConf = conf.Get<MlApiConfiguration>();
        services.AddSingleton<IFluffleMachineLearningApiClient>(new FluffleMachineLearningApiClient(mlApiConf.Url, mlApiConf.ApiKey));

        // Database
        services.AddTwitterDatabase(syncConf.MongoDb.ConnectionString, syncConf.MongoDb.Database);

        // Twitter API client
        var twitterApiConf = conf.Get<TwitterApiConfiguration>();
        services.AddSingleton<ITwitterApiClient>(new TwitterApiClient(twitterApiConf.Url, twitterApiConf.ApiKey));

        // Main API client
        var mainConf = conf.Get<MainConfiguration>();
        services.AddSingleton(new FluffleClient(mainConf.Url, mainConf.ApiKey));

        // Queues
        var queueyConf = conf.Get<QueueyConfiguration>();
        services.UseQueuey(queueyConf.Url, queueyConf.ApiKey);
        services.AddQueue<ImportUserQueueItem>("TwitterImportUser");
        services.AddQueue<UserCheckFurryQueueItem>("TwitterCheckFurry");
        services.AddQueue<MediaIngestQueueItem>("TwitterIngestMedia");

        // Services
        services.AddSingleton<IUserService, UserService>();

        return services;
    }
}