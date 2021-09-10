using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class ArchiveStrategy : FurAffinityContentProducerStrategy
    {
        private DateTimeOffset? NextSubmissionAt { get; set; }
        private static DateTimeOffset ArchiveUntil => DateTimeOffset.UtcNow.AddDays(-7);

        public ArchiveStrategy(FluffleClient fluffleClient, FurAffinityClient faClient,
            FurAffinitySyncClientState state) : base(fluffleClient, faClient, state)
        {
        }

        public override async Task<FurAffinityContentProducerStateResult> NextAsync()
        {
            if (NextSubmissionAt == null || NextSubmissionAt <= DateTimeOffset.UtcNow)
            {
                var result = await NextAsync(++State.ArchiveStartId);

                if (result.FaResult == null)
                    return result;

                if (ArchiveUntil < result.FaResult.Result.When)
                    NextSubmissionAt = DateTimeOffset.UtcNow.AddMinutes(15);

                return result;
            }

            if (State.ArchiveEndId == 1)
                return null;

            return await NextAsync(--State.ArchiveEndId);
        }

        private async Task<FurAffinityContentProducerStateResult> NextAsync(int id)
        {
            var getSubmissionResult = await LogEx.TimeAsync(async () =>
            {
                return await HttpResiliency.RunAsync(() => FaClient.GetSubmissionAsync(id));
            }, "Retrieving submission with ID {id}", id);

            return new FurAffinityContentProducerStateResult
            {
                FaResult = getSubmissionResult
            };
        }
    }
}
