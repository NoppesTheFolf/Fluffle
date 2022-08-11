using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Database.Models;
using ApiKey = Noppes.Fluffle.Main.Database.Models.ApiKey;
using ApiKeyPermission = Noppes.Fluffle.Main.Database.Models.ApiKeyPermission;
using Permission = Noppes.Fluffle.Main.Database.Models.Permission;

namespace Noppes.Fluffle.Main.Api
{
    public class Startup : ApiStartup<Startup, FluffleContext>
    {
        private const string UserAgentApplicationName = "main";

        protected override bool EnableAccessControl => true;

        public override void AdditionalConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            var mainConf = Configuration.Get<MainServerConfiguration>();
            services.AddSingleton(mainConf);

            services.AddDatabase<FluffleContext, MainDatabaseConfiguration>(Configuration);

            services.AddSingleton<TagBlacklistCollection>();

            services.AddSingleton<IndexStatisticsService>();
            services.AddSingleton<CreditableEntityPriorityService>();
            services.AddSingleton<DeletionService>();

            var b2Conf = Configuration.Get<BackblazeB2Configuration>();
            var b2Client = new B2Client(b2Conf.ApplicationKeyId, b2Conf.ApplicationKey);
            services.AddSingleton(b2Client);
            var bucket = b2Client.GetBucketAsync().Result;
            services.AddSingleton(bucket);

            var faConf = Configuration.Get<FurAffinityConfiguration>();
            var faClient = new FurAffinityClient("https://www.furaffinity.net", Project.UserAgent(UserAgentApplicationName), faConf.A, faConf.B);
            services.AddSingleton(faClient);

            services.RegisterChangeIdIncrementers();

            services.AddTransient<ApiInitializer>();
        }

        public override void ConfigureAuthentication(IServiceCollection services, AuthenticationBuilder authenticationBuilder)
        {
            authenticationBuilder.AddApiKeySupport<FluffleContext, ApiKey, Permission, ApiKeyPermission>(_ => { }, services);
        }

        public override void AfterConfigure(IApplicationBuilder app, IWebHostEnvironment env, ServiceBuilder serviceBuilder)
        {
            var blacklistConf = Configuration.Get<BlacklistConfiguration>();
            var tagBlacklist = app.ApplicationServices.GetRequiredService<TagBlacklistCollection>();
            tagBlacklist.Initialize(blacklistConf.Universal, blacklistConf.Nsfw);

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<ApiInitializer>();
                initializer.InitializeAsync().Wait();
            }

            app.ApplicationServices.InitializeChangeIdIncrementers();

            var mainConf = Configuration.Get<MainServerConfiguration>();
            serviceBuilder.AddSingleton<IndexStatisticsService>(mainConf.IndexingStatisticsInterval.Minutes());
            serviceBuilder.AddSingleton<CreditableEntityPriorityService>(mainConf.CreditableEntityPriorityInterval.Minutes());
            serviceBuilder.AddTransient<DeletionService>(mainConf.DeletionInterval.Minutes());
        }
    }
}
