using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api
{
    public class Startup : ApiStartup<Startup, FluffleSearchContext>
    {
        protected override bool EnableAccessControl => true;

        public override void AdditionalConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FluffleSearchContext>(options =>
            {
                options.UseNpgsql(Configuration.Get<SearchDatabaseConfiguration>().ConnectionString);
            });

            var mainConf = Configuration.Get<MainConfiguration>();
            services.AddSingleton(new FluffleClient(mainConf.Url, mainConf.ApiKey));

            var fluffleHash = new FluffleHash();
            services.AddSingleton(fluffleHash);
            services.AddSingleton(services => new FluffleHashSelfTestRunner(fluffleHash)
            {
                Log = message => services
                    .GetRequiredService<ILogger<FluffleHashSelfTestRunner>>()
                    .LogInformation(message)
            });
            services.AddSingleton<PlatformSearchService>();

            services.AddTransient<HashRefresher>();
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

            serviceBuilder.AddSingleton<HashRefresher>(60.Seconds());
            serviceBuilder.AddSingleton<SyncService>(2.Minutes());

            base.AfterConfigure(app, env, serviceBuilder);
        }
    }
}
