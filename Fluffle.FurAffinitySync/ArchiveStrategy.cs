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
        private static DateTimeOffset ArchiveUntil => DateTimeOffset.UtcNow.AddDays(-14);

        public ArchiveStrategy(FluffleClient fluffleClient, FurAffinityClient faClient,
            FurAffinitySyncClientState state) : base(fluffleClient, faClient, state)
        {
        }

        public override async Task<(int?, FurAffinityContentProducerStateResult)> NextAsync()
        {
            if (NextSubmissionAt == null || NextSubmissionAt <= DateTimeOffset.UtcNow)
            {
                var submissionIdPositive = ++State.ArchiveStartId;
                var result = await NextAsync(submissionIdPositive);

                if (result.FaResult == null)
                    return (submissionIdPositive, result);

                if (ArchiveUntil < result.FaResult.Result.When)
                    NextSubmissionAt = DateTimeOffset.UtcNow.AddMinutes(15);

                return (submissionIdPositive, result);
            }

            if (State.ArchiveEndId <= 0)
                return (null, null);

            var submissionIdNegative = --State.ArchiveEndId;
            return (submissionIdNegative, await NextAsync(submissionIdNegative));
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
