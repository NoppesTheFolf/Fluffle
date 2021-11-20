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
using static MoreLinq.Extensions.BatchExtension;

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

        public Task<int> GetClassifyBatchSizeAsync();

        public Task<bool> IsFurryArtistAsync(IEnumerable<AnalyzeScore> scores);

        public Task<ICollection<bool>> IsFurryArtAsync(IEnumerable<IDictionary<ClassificationClass, double>> classes);
    }

    public class PredictionClient : ApiClient, IPredictionClient
    {
        private readonly SemaphoreInterceptor _classifyInterceptor;

        public PredictionClient(string baseUrl, string apiKey, int classifyDegreeOfParallelism) : base(baseUrl)
        {
            FlurlClient.WithHeader("User-Agent", Project.UserAgent);
            FlurlClient.WithHeader("Api-Key", apiKey);
            FlurlClient.Settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(true, false),
                }
            });

            _classifyInterceptor = new SemaphoreInterceptor(classifyDegreeOfParallelism);
        }

        public async Task<ICollection<IDictionary<ClassificationClass, double>>> ClassifyAsync(IEnumerable<Func<Stream>> streams)
        {
            var results = new List<IDictionary<ClassificationClass, double>>();

            var batchSize = await GetClassifyBatchSizeAsync();
            foreach (var batch in streams.Batch(batchSize))
            {
                var response = await Request("image-classifier")
                    .AddInterceptor(_classifyInterceptor)
                    .PostMultipartAsync(content =>
                    {
                        foreach (var stream in batch)
                            content.AddFile("files", stream(), "dummy");
                    });

                var result = await response.GetJsonAsync<ICollection<IDictionary<ClassificationClass, double>>>();
                results.AddRange(result);
            }

            return results;
        }

        public Task<int> GetClassifyBatchSizeAsync()
        {
            return Request("image-classifier/batch-size").GetJsonAsync<int>();
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
