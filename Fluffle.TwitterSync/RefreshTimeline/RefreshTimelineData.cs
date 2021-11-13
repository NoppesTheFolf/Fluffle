using Noppes.Fluffle.TwitterSync.AnalyzeUsers;

namespace Noppes.Fluffle.TwitterSync.RefreshTimeline
{
    public class RefreshTimelineData : IUserTweetsSupplierData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public TimelineCollection Timeline { get; set; }
    }
}
