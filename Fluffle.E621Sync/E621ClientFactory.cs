using Humanizer;
using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.E621Sync
{
    public class E621ClientFactory : ClientFactory<IE621Client>
    {
        public E621ClientFactory(FluffleConfiguration configuration) : base(configuration)
        {
        }

        public override async Task<IE621Client> CreateAsync(string productName, int interval)
        {
            var contactConfiguration = Configuration.Get<ContactConfiguration>();
            var e621Configuration = Configuration.Get<E621Configuration>();

            var e621Client = new E621ClientBuilder()
                .WithUserAgent(productName, Project.Version, contactConfiguration.Username, contactConfiguration.Platform)
                .WithBaseUrl(Imageboard.E621)
                .WithMaximumConnections(1)
                .WithRequestInterval(interval.Milliseconds())
                .Build();

            var loginSuccess = await HttpResiliency.RunAsync(() =>
                e621Client.LogInAsync(e621Configuration.Username, e621Configuration.ApiKey));

            if (!loginSuccess)
            {
                Log.Fatal("Invalid e621 credentials.");
                Environment.Exit(-1);
            }

            return e621Client;
        }
    }
}
