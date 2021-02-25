using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using Serilog;
using System;
using System.Threading.Tasks;
using Noppes.Fluffle.Constants;

namespace Noppes.Fluffle.E621Sync
{
    public class E621ClientFactory : ClientFactory<E621Client>
    {
        public E621ClientFactory(FluffleConfiguration configuration) : base(configuration)
        {
        }

        public override async Task<E621Client> CreateAsync(string productName)
        {
            var contactConfiguration = Configuration.Get<ContactConfiguration>();
            var e621Configuration = Configuration.Get<E621Configuration>();

            var e621Client = new E621ClientBuilder()
                .WithUserAgent(productName, Project.Version, contactConfiguration.Username, contactConfiguration.Platform)
                .WithBaseUrl(Imageboard.E621)
                .WithMaximumConnections(1)
                .WithRequestInterval(E621Client.RecommendedRequestInterval)
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
