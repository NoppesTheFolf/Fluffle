using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class RecentSubmissionConsumer : Consumer<RecentSubmissionData>
    {
        private readonly FurAffinityClient _furAffinityClient;
        private readonly FluffleClient _fluffleClient;
        private readonly FurAffinityContentProducer _contentProducer;
        private readonly FurAffinitySyncConfiguration _configuration;

        public RecentSubmissionConsumer(FurAffinityClient furAffinityClient, FluffleClient fluffleClient,
            FurAffinityContentProducer contentProducer, FurAffinitySyncConfiguration configuration)
        {
            _furAffinityClient = furAffinityClient;
            _fluffleClient = fluffleClient;
            _contentProducer = contentProducer;
            _configuration = configuration;
        }

        public override async Task<RecentSubmissionData> ConsumeAsync(RecentSubmissionData data)
        {
            async Task GetSubmissionAsync()
            {
                data.Submission = await HttpResiliency.RunAsync(() => _furAffinityClient.GetSubmissionAsync(data.Id));
            }

            if (data.ArtistId == null)
            {
                await GetSubmissionAsync();
                if (data.Submission == null)
                    return null;

                data.ArtistId = data.Submission.Result.Owner.Id;
            }

            var priority = await _fluffleClient.GetCreditableEntitiesMaxPriority(Enum.GetName(PlatformConstant.FurAffinity), data.ArtistId);
            if (priority == null || priority < _configuration.RecentSubmissionPriorityThreshold)
                return null;

            if (data.Submission == null)
            {
                await GetSubmissionAsync();
                if (data.Submission == null)
                    return null;
            }

            if (!_contentProducer.ShouldBeIndexed(data.Submission.Result))
                return null;

            await _contentProducer.SubmitContentAsync(new List<FaSubmission> { data.Submission.Result });
            return data;
        }
    }
}
