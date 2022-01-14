using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class RecentSubmissionData
    {
        public int Id { get; set; }

        public string ArtistId { get; set; }

        public FaResult<FaSubmission> Submission { get; set; }
    }

    public class RecentSubmissionProducer : Producer<RecentSubmissionData>
    {
        private int? _previousLatestSubmissionId;

        private readonly FurAffinityClient _client;
        private readonly FurAffinitySyncConfiguration _configuration;

        public RecentSubmissionProducer(FurAffinityClient client, FurAffinitySyncConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public override async Task WorkAsync()
        {
            async Task ProduceSubmissionAsync(FaGallerySubmission submission)
            {
                await ProduceAsync(new RecentSubmissionData
                {
                    Id = submission.Id,
                    ArtistId = submission.ArtistId
                });
            }

            var result = await LogEx.TimeAsync(() =>
            {
                return HttpResiliency.RunAsync(() => _client.GetRecentSubmissions());
            }, "Getting recent submissions");
            var submissions = result.ToDictionary(s => s.Id);
            var latestSubmissionId = submissions.Keys.Max();

            if (_previousLatestSubmissionId != null)
            {
                var previousLatestSubmissionId = (int)_previousLatestSubmissionId;
                foreach (var submissionId in Enumerable.Range(previousLatestSubmissionId + 1, latestSubmissionId - previousLatestSubmissionId))
                {
                    if (submissions.TryGetValue(submissionId, out var submission))
                    {
                        await ProduceSubmissionAsync(submission);
                        continue;
                    }

                    Log.Warning("Missing submission with ID {submissionId}", submissionId);
                    await ProduceSubmissionAsync(new FaGallerySubmission
                    {
                        Id = submissionId
                    });
                }
            }
            else
            {
                foreach (var submission in submissions.Values)
                {
                    await ProduceSubmissionAsync(submission);
                }
            }

            _previousLatestSubmissionId = latestSubmissionId;
            await Task.Delay(_configuration.RecentSubmissionsInterval);
        }
    }
}
