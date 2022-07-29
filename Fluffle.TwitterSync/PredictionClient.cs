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
    public interface IPredictionClient
    {
        public Task VerifyImage(Stream stream);

        public Task<ICollection<IDictionary<bool, double>>> ClassifyAsync(IEnumerable<Func<Stream>> streams);

        public Task<int> GetClassifyBatchSizeAsync();

        public Task<bool> IsFurryArtistAsync(IEnumerable<IDictionary<bool, double>> classes, IEnumerable<int> artistIds);

        public Task<ICollection<bool>> IsFurryArtAsync(IEnumerable<IDictionary<bool, double>> classes);
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

        public async Task VerifyImage(Stream stream)
        {
            await Request("verify-image").PostMultipartAsync(content =>
            {
                content.AddFile("file", stream, "dummy");
            });
        }

        public async Task<ICollection<IDictionary<bool, double>>> ClassifyAsync(IEnumerable<Func<Stream>> streams)
        {
            var results = new List<IDictionary<bool, double>>();

            var batchSize = await GetClassifyBatchSizeAsync();
            foreach (var batch in streams.Batch(batchSize))
            {
                var response = await Request("image-classifier-v2")
                    .AddInterceptor(_classifyInterceptor)
                    .PostMultipartAsync(content =>
                    {
                        foreach (var stream in batch)
                            content.AddFile("files", stream(), "dummy");
                    });

                var result = await response.GetJsonAsync<ICollection<IDictionary<bool, double>>>();
                results.AddRange(result);
            }

            return results;
        }

        public Task<int> GetClassifyBatchSizeAsync()
        {
            return Request("image-classifier-v2/batch-size").GetJsonAsync<int>();
        }

        public async Task<ICollection<bool>> IsFurryArtAsync(IEnumerable<IDictionary<bool, double>> classes)
        {
            var response = await Request("is-furry-art-v2").PostJsonAsync(classes);

            return await response.GetJsonAsync<ICollection<bool>>();
        }

        public async Task<bool> IsFurryArtistAsync(IEnumerable<IDictionary<bool, double>> classes, IEnumerable<int> artistIds)
        {
            var response = await Request("is-furry-artist-v2").PostJsonAsync(new
            {
                Classes = classes,
                ArtistIds = artistIds
            });

            return await response.GetJsonAsync<bool>();
        }
    }
}
