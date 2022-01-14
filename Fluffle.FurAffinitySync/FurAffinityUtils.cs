using Humanizer;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class FurAffinityUtils
    {
        private static readonly TimeSpan CheckInterval = 5.Minutes();

        public static async Task WaitTillAllowedAsync<T>(FaResult<T> result, IHostEnvironment environment, FluffleClient fluffleClient)
        {
            if (result == null || result.Stats.Registered < FurAffinityClient.BotThreshold)
                return;

            if (environment.IsDevelopment())
                return;

            bool allowedToContinue;
            do
            {
                Log.Information("No bots allowed at this moment. Waiting for {time} before checking again.", CheckInterval.Humanize());
                await Task.Delay(CheckInterval);

                allowedToContinue = await HttpResiliency.RunAsync(fluffleClient.GetFaBotsAllowedAsync);
            } while (!allowedToContinue);
            Log.Information("Bots allowed again, continuing full sync...");
        }
    }
}
