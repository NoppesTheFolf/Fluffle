using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Utils;
using Serilog;
using SerilogTimings;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class GetSubmissionSchedulerItem : WorkSchedulerItem<FaResult<FaSubmission>>
    {
        public int SubmissionId { get; set; }
    }

    public class GetSubmissionScheduler : WorkScheduler<GetSubmissionSchedulerItem, int, FaResult<FaSubmission>>
    {
        private readonly FurAffinityClient _furAffinityClient;
        private readonly FurAffinitySyncConfiguration _configuration;
        private int _interval;

        public GetSubmissionScheduler(FurAffinityClient furAffinityClient, FurAffinitySyncConfiguration configuration) : base(1)
        {
            _furAffinityClient = furAffinityClient;
            _configuration = configuration;
            _interval = configuration.AboveBotLimitInterval;
        }

        protected override int? GetInterval()
        {
            return _interval;
        }

        protected override async Task<FaResult<FaSubmission>> HandleAsync(GetSubmissionSchedulerItem item)
        {
            return await _furAffinityClient.GetSubmissionAsync(item.SubmissionId);
        }

        public override async Task<FaResult<FaSubmission>> ProcessAsync(GetSubmissionSchedulerItem item, int priority)
        {
            Log.Information("Scheduled retrieving submission {id} with priority {priority}", item.SubmissionId, priority);
            using var _ = Operation.Time("Retrieving submission {id} with priority {priority}", item.SubmissionId, priority);
            var result = await base.ProcessAsync(item, priority);

            var stats = result?.Stats;
            if (stats != null)
            {
                _interval = stats.Registered < FurAffinityClient.BotThreshold
                    ? _configuration.BelowBotLimitInterval
                    : _configuration.AboveBotLimitInterval;
            }

            return result;
        }
    }
}
