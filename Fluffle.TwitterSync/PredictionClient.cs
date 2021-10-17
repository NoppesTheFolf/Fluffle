using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync
{
    public enum ClassificationClass
    {
        Anime,
        FurryArt,
        Fursuit,
        Real
    }

    public class AnalyzeScore
    {
        public double FurryArt { get; set; }

        public double Real { get; set; }

        public double Fursuit { get; set; }

        public double Anime { get; set; }

        public int[] ArtistIds { get; set; }
    }

    public interface IPredictionClient
    {
        public Task<ICollection<IDictionary<ClassificationClass, double>>> ClassifyAsync(IEnumerable<Func<Stream>> streams);

        public Task<bool> IsFurryArtistAsync(IEnumerable<AnalyzeScore> scores);

        public Task<ICollection<bool>> IsFurryArtAsync(IEnumerable<IDictionary<ClassificationClass, double>> classes);
    }

    public class PredictionClient : ApiClient, IPredictionClient
    {
        public PredictionClient(string baseUrl) : base(baseUrl)
        {
            FlurlClient.WithHeader("User-Agent", Project.UserAgent);
            FlurlClient.Settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(true, false),
                }
            });
        }

        public async Task<ICollection<IDictionary<ClassificationClass, double>>> ClassifyAsync(IEnumerable<Func<Stream>> streams)
        {
            var response = await Request("image-classifier").PostMultipartAsync(content =>
            {
                foreach (var stream in streams)
                    content.AddFile("files", stream(), "dummy");
            });

            return await response.GetJsonAsync<ICollection<IDictionary<ClassificationClass, double>>>();
        }

        public async Task<bool> IsFurryArtistAsync(IEnumerable<AnalyzeScore> scores)
        {
            var response = await Request("is-furry-artist").PostJsonAsync(scores);

            return await response.GetJsonAsync<bool>();
        }

        public async Task<ICollection<bool>> IsFurryArtAsync(IEnumerable<IDictionary<ClassificationClass, double>> classes)
        {
            var response = await Request("is-furry-art").PostJsonAsync(classes);

            return await response.GetJsonAsync<ICollection<bool>>();
        }
    }
}
