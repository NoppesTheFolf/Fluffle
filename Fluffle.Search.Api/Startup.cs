using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api
{
    public class Startup : ApiStartup<Startup, FluffleSearchContext>
    {
        protected override bool EnableAccessControl => false;

        public override void AdditionalConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FluffleSearchContext>(options =>
            {
                options.UseNpgsql(Configuration.Get<SearchDatabaseConfiguration>().ConnectionString);
            });

            var mainConf = Configuration.Get<MainConfiguration>();
            services.AddSingleton(new FluffleClient(mainConf.Url, mainConf.ApiKey));

            services.AddSingleton<FluffleHash>();
            services.AddSingleton<PlatformSearchService>();

            services.AddTransient<HashRefresher>();
            services.AddSingleton<SyncService>();
        }

        public override void AfterConfigure(IApplicationBuilder app, IWebHostEnvironment env, ServiceBuilder serviceBuilder)
        {
            serviceBuilder.AddSingleton<HashRefresher>(60.Seconds());
            serviceBuilder.AddSingleton<SyncService>(2.Minutes());
        }
    }
}
