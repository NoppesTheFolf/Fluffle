using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Api.LinkCreation;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Thumbnail;

namespace Noppes.Fluffle.Search.Api
{
    public class B2ClientCollection
    {
        public B2Bucket SearchResultsClient { get; set; }
    }

    public class Startup : ApiStartup<Startup, FluffleSearchContext>
    {
        protected override bool EnableAccessControl => true;

        public override void AdditionalConfigureServices(IServiceCollection services)
        {
            services.AddDatabase<FluffleSearchContext, SearchDatabaseConfiguration>(Configuration);

            var conf = Configuration.Get<SearchServerConfiguration>();
            services.AddSingleton(conf);

            var searchResultsClient = new B2Client(conf.SearchResultsBackblazeB2.ApplicationKeyId, conf.SearchResultsBackblazeB2.ApplicationKey);
            services.AddSingleton(new B2ClientCollection
            {
                SearchResultsClient = searchResultsClient.GetBucketAsync().Result,
            });

            services.AddSingleton<LinkCreatorStorage>();
            services.AddHostedService<LinkCreator>();
            services.AddSingleton<LinkCreatorRetriever>();
            services.AddSingleton<LinkCreatorUploader>();
            services.AddSingleton<LinkCreatorUpdater>();

            var mainConf = Configuration.Get<MainConfiguration>();
            services.AddSingleton(new FluffleClient(mainConf.Url, mainConf.ApiKey));

            services.AddFluffleThumbnail();

            var fluffleHash = new FluffleHash();
            services.AddSingleton(fluffleHash);
            services.AddSingleton(services => new FluffleHashSelfTestRunner(fluffleHash)
            {
                Log = message => services
                    .GetRequiredService<ILogger<FluffleHashSelfTestRunner>>()
                    .LogInformation(message)
            });

            var compareConf = Configuration.Get<CompareConfiguration>();
            var compareClient = new CompareClient(compareConf.Url);
            services.AddSingleton<ICompareClient>(compareClient);

            services.AddSingleton<SyncService>();
        }

        public override void ConfigureAuthentication(IServiceCollection services, AuthenticationBuilder authenticationBuilder)
        {
            authenticationBuilder.AddApiKeySupport<FluffleSearchContext, ApiKey, Permission, ApiKeyPermission>(_ => { }, services);
        }

        public override void AfterConfigure(IApplicationBuilder app, IWebHostEnvironment env, ServiceBuilder serviceBuilder)
        {
            if (env.IsProduction())
                app.ApplicationServices.GetRequiredService<FluffleHashSelfTestRunner>().Run();

            serviceBuilder.AddSingleton<SyncService>(2.Minutes());

            base.AfterConfigure(app, env, serviceBuilder);
        }
    }
}
