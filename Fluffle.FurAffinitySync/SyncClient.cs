﻿using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Sync;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    internal class SyncClient : SyncClient<SyncClient, FurAffinityContentProducer, FaSubmission>
    {
        private const string UserAgentApplicationName = "fur-affinity-sync";

        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await RunAsync(args, "Fur Affinity", (configuration, services) =>
        {
            var syncConf = configuration.Get<FurAffinitySyncConfiguration>();
            services.AddSingleton(syncConf);
            services.AddFurAffinityClient(configuration, syncConf.Interval, UserAgentApplicationName);

            services.AddSingleton<GetSubmissionScheduler>();
        });
    }
}
