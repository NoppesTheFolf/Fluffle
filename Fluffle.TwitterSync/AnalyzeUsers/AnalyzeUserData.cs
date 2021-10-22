using System;
using System.Collections.Generic;
using System.IO;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class AnalyzeUserData : IPredictClassesData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public TimelineCollection Timeline { get; set; }

        public ICollection<RetrieverImage> Images { get; set; }
        public ICollection<Stream> Streams { get; set; }
        public ICollection<Func<Stream>> OpenStreams { get; set; }

        public ICollection<IDictionary<ClassificationClass, double>> Classes { get; set; }

        public ICollection<FluffleResult> BestMatches { get; set; }

        public bool IsFurryArtist { get; set; }
    }
}
