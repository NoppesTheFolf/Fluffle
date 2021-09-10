using System.Collections.Generic;
using System.Text.Json.Serialization;

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
            public int Id { get; set; }

            public double Score { get; set; }

            public string Platform { get; set; }

            public string Location { get; set; }

            public bool IsSfw { get; set; }

            public class ThumbnailModel
            {
                public int Id { get; set; }

                public int Width { get; set; }

                public int CenterX { get; set; }

                public int Height { get; set; }

                public int CenterY { get; set; }

                public string Location { get; set; }
            }

            public ThumbnailModel Thumbnail { get; set; }

            public class CreditModel
            {
                public int Id { get; set; }

                public string Name { get; set; }
            }

            public IEnumerable<CreditModel> Credits { get; set; }

            public class StatsModel
            {
                public int Average64 { get; set; }

                public int Red256 { get; set; }

                public int Green256 { get; set; }

                public int Blue256 { get; set; }

                public int Red1024 { get; set; }

                public int Green1024 { get; set; }

                public int Blue1024 { get; set; }
            }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public StatsModel Stats { get; set; }
        }

        public IEnumerable<ImageModel> Results { get; set; }
    }
}
