using Noppes.Fluffle.TwitterSync.AnalyzeUsers;
using Noppes.Fluffle.TwitterSync.Database.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Noppes.Fluffle.TwitterSync.AnalyzeMedia
{
    public class AnalyzeMediaResult
    {
        public Media Media { get; set; }

        public Tweet Tweet { get; set; }
    }

    public class AnalyzeMediaData : IPredictClassesData
    {
        public ICollection<string> TweetIds { get; set; }

        public ICollection<RetrieverImage> Images { get; set; }
        public ICollection<Stream> Streams { get; set; }
        public ICollection<Func<Stream>> OpenStreams { get; set; }

        public ICollection<IDictionary<bool, double>> Classes { get; set; }

        public ICollection<bool> IsFurryArt { get; set; }
    }
}
