using Humanizer;
using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
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

        public override async Task<IE621Client> CreateAsync(int interval, string applicationName)
        {
            var conf = Configuration.Get<E621Configuration>();

            var e621Client = new E621ClientBuilder()
                .WithUserAgent(Project.UserAgentBase(applicationName), Project.Version, Project.DeveloperUsername, Project.DeveloperUrl)
                .WithBaseUrl(Imageboard.E621)
                .WithMaximumConnections(1)
                .WithRequestInterval(interval.Milliseconds())
                .Build();

            var loginSuccess = await E621HttpResiliency.RunAsync(() => e621Client.LogInAsync(conf.Username, conf.ApiKey, true));

            if (!loginSuccess)
            {
                Log.Fatal("Invalid e621 credentials.");
                Environment.Exit(-1);
            }

            return e621Client;
        }
    }
}
