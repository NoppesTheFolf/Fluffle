using Noppes.Fluffle.Constants;
using System.Collections.Generic;

namespace Noppes.Fluffle.Search.Api.Models
{
    public class SearchResultModel
    {
        public class StatsModel
        {
            public int Count { get; set; }

            public int ElapsedMilliseconds { get; set; }
        }

        public StatsModel Stats { get; set; }

        public class ImageModel
        {
            public double Score { get; set; }

            public string Platform { get; set; }

            public string ViewLocation { get; set; }

            public bool IsSfw { get; set; }

            public class ThumbnailModel
            {
                public int Width { get; set; }

                public int CenterX { get; set; }

                public int Height { get; set; }

                public int CenterY { get; set; }

                public string Location { get; set; }
            }

            public ThumbnailModel Thumbnail { get; set; }

            public IEnumerable<string> Credits { get; set; }
        }

        public IEnumerable<ImageModel> Results { get; set; }
    }
}
