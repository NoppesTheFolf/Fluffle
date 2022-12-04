using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.DeviantArt.Client;
using Noppes.Fluffle.DeviantArt.Database;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.KeyValue;
using Noppes.Fluffle.KeyValue.Azure;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Queue.Azure;

namespace Noppes.Fluffle.DeviantArt.Shared;

public static class ServiceCollectionExtensions
{
    public static void AddDeviantArt<TSubConf>(this IServiceCollection services, FluffleConfiguration conf,
        Func<DeviantArtConfiguration, TSubConf> selectSubconf, bool addDatabase, bool addClient, bool addQueues, bool addKeyValueStore) where TSubConf : DeviantArtApplicationConfiguration<TSubConf>
    {
        var daConf = conf.Get<DeviantArtConfiguration>();
        services.AddSingleton(daConf);

        var subConf = selectSubconf(daConf);
        if (subConf == null)
            throw new InvalidOperationException("Subconfiguration could not be loaded.");
        services.AddSingleton(subConf);

        if (daConf.Tags != null)
        {
            var tags = new DeviantArtTags(daConf.Tags.Furry, daConf.Tags.General);
            services.AddSingleton(tags);
        }

        if (addDatabase)
        {
            services.AddDatabase<DeviantArtContext, DeviantArtDatabaseConfiguration>(conf);
        }

        if (addClient)
        {
            var userAgent = Project.UserAgent("deviant-art-sync");
            var client = new DeviantArtClient(daConf.Credentials.ClientId, daConf.Credentials.ClientSecret, userAgent);
            if (subConf.ClientThrottle != null)
                client.RateLimiter = new RequestRateLimiter(((int)subConf.ClientThrottle).Milliseconds());

            services.AddSingleton(client);
        }

        if (addQueues)
        {
            services.UseStorageQueue(daConf.StorageAccount.ConnectionString);
            services.AddQueue<ProcessDeviationQueueItem>("process-deviation");
            services.AddSingleton<ProcessDeviationQueue>();
            services.AddQueue<CheckIfFurryArtistQueueItem>("check-if-furry-artist");
            services.AddQueue<ScrapeGalleryQueueItem>("scrape-gallery");
        }

        if (addKeyValueStore)
        {
            services.UseTableStorage(daConf.StorageAccount.ConnectionString);
            services.AddKeyValueStore();

            services.AddSingleton<IQueryDeviationsLatestPublishedWhenStore, QueryDeviationsLatestPublishedWhenStore>();
            services.AddSingleton<INewestDeviationsLatestPublishedWhenStore, NewestDeviationsLatestPublishedWhenStore>();
        }
    }
}