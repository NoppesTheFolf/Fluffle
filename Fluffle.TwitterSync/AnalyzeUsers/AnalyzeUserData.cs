using System;
using System.Collections.Generic;
using System.IO;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public interface IUserTweetsSupplierData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public TimelineCollection Timeline { get; set; }
        public DateTimeOffset TimelineRetrievedAt { get; set; }
    }

    public class AnalyzeUserData : IUserTweetsSupplierData, IPredictClassesData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public TimelineCollection Timeline { get; set; }
        public DateTimeOffset TimelineRetrievedAt { get; set; }

        public ICollection<RetrieverImage> Images { get; set; }
        public ICollection<Stream> Streams { get; set; }
        public ICollection<Func<Stream>> OpenStreams { get; set; }

        public ICollection<IDictionary<bool, double>> Classes { get; set; }

        public ICollection<FluffleResult> BestMatches { get; set; }
    }
}
