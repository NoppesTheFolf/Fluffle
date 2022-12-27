using Flurl.Http;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Client
{
    public class FluffleClient : ApiClient
    {
        private const string ApiKeyHeader = "Api-Key";

        private readonly string _apiKey;

        public FluffleClient(string baseUrl, string apiKey) : base(baseUrl)
        {
            _apiKey = apiKey;
        }

        public Task PutSyncStateAsync(string platformName, SyncStateModel model)
        {
            return Request(Endpoints.SyncState(platformName))
                .PutJsonAsync(model);
        }

        public Task<SyncStateModel> GetSyncStateAsync(string platformName)
        {
            return Request(Endpoints.SyncState(platformName))
                .GetJsonAsync<SyncStateModel>();
        }

        public Task<bool> GetFaBotsAllowedAsync()
        {
            return Request(Endpoints.GetFaBotsAllowed())
                .GetJsonAsync<bool>();
        }

        public Task<IList<FaPopularArtistModel>> GetFaPopularArtistsAsync()
        {
            return Request(Endpoints.GetFaPopularArtists())
                .GetMessagePackAsync<IList<FaPopularArtistModel>>();
        }

        public async Task<ICollection<string>> SearchContentAsync(string platformName, IEnumerable<string> idStartsWithMany)
        {
            var allIds = new ConcurrentBag<string>();
            await Parallel.ForEachAsync(idStartsWithMany, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (idStartsWith, _) =>
            {
                var ids = await SearchContentAsync(platformName, idStartsWith);
                foreach (var id in ids)
                    allIds.Add(id);
            });

            return allIds.Distinct().ToList();
        }

        public Task<ICollection<string>> SearchContentAsync(string platformName, string idStartsWith)
        {
            return Request(Endpoints.SearchContent(platformName))
                .SetQueryParam("idStartsWith", idStartsWith)
                .GetJsonAsync<ICollection<string>>();
        }

        public Task PutContentAsync(string platformName, IEnumerable<PutContentModel> models)
        {
            return Request(Endpoints.PutContent(platformName))
                .PutJsonAsync(models);
        }

        public Task PutContentWarningAsync(string platformName, string platformContentId, PutWarningModel model)
        {
            return Request(Endpoints.PutContentWarning(platformName, platformContentId))
                .PutJsonAsync(model);
        }

        public Task PutContentErrorAsync(string platformName, string platformContentId, PutErrorModel model)
        {
            return Request(Endpoints.PutContentError(platformName, platformContentId))
                .PutJsonAsync(model);
        }

        public Task IndexImageAsync(string platformName, string platformImageId, PutImageIndexModel model)
        {
            return Request(Endpoints.PutImageIndex(platformName, platformImageId))
                .PutJsonAsync(model);
        }

        public Task<ICollection<int>> DeleteContentRangeAsync(string platformName, DeleteContentRangeModel model)
        {
            return Request(Endpoints.DeleteContentRange(platformName))
                .DeleteJsonReceiveJsonAsync<ICollection<int>>(model);
        }

        public Task<ICollection<string>> DeleteContentAsync(string platformName, IEnumerable<string> platformContentIds)
        {
            return Request(Endpoints.DeleteContent(platformName))
                .DeleteJsonReceiveJsonAsync<ICollection<string>>(platformContentIds);
        }

        public Task<PlatformModel> GetPlatformAsync(string name)
        {
            return Request(Endpoints.GetPlatform(name))
                .GetJsonAsync<PlatformModel>();
        }

        public Task SignalPlatformSyncAsync(string platformName, SyncTypeConstant syncType)
        {
            return Request(Endpoints.SignalPlatformSync(platformName, syncType))
                .PutAsync();
        }

        public Task<IList<PlatformModel>> GetPlatformsAsync()
        {
            return Request(Endpoints.GetPlatforms)
                .GetJsonAsync<IList<PlatformModel>>();
        }

        public Task<IList<UnprocessedImageModel>> GetUnprocessedImagesAsync(string platformName)
        {
            return Request(Endpoints.GetUnprocessedImages(platformName))
                .GetMessagePackAsync<IList<UnprocessedImageModel>>();
        }

        public Task<string> GetContentToRetryAsync(string platformName)
        {
            return Request(Endpoints.GetContentToRetry(platformName)).GetJsonAsync<string>();
        }

        public Task<IList<StatusModel>> GetStatusAsync()
        {
            return Request(Endpoints.GetStatus)
                .GetJsonAsync<IList<StatusModel>>();
        }

        public Task<int?> GetMinId(string platformName)
        {
            return Request(Endpoints.GetMinId(platformName))
                .GetJsonAsync<int?>();
        }

        public Task<int?> GetMaxId(string platformName)
        {
            return Request(Endpoints.GetMaxId(platformName))
                .GetJsonAsync<int?>();
        }

        public Task<PlatformSyncModel> GetPlatformSync(string platformName)
        {
            return Request(Endpoints.GetPlatformSync(platformName))
                .GetJsonAsync<PlatformSyncModel>();
        }

        public Task<ImagesSyncModel> GetSyncImagesAsync(string platformName, long afterChangeId)
        {
            return Request(Endpoints.GetSyncImages(platformName, afterChangeId))
                .GetMessagePackAsync<ImagesSyncModel>();
        }

        public Task<CreditableEntitiesSyncModel> GetSyncCreditableEntitiesAsync(string platformName, long afterChangeId)
        {
            return Request(Endpoints.GetSyncCreditableEntities(platformName, afterChangeId))
                .GetMessagePackAsync<CreditableEntitiesSyncModel>();
        }

        public Task<ICollection<OtherSourceModel>> GetOtherSourcesAsync(int afterId)
        {
            return Request(Endpoints.GetOtherSources(afterId))
                .GetMessagePackAsync<ICollection<OtherSourceModel>>();
        }

        public Task<int?> GetCreditableEntitiesMaxPriority(string platformName, string creditableEntityName)
        {
            return Request(Endpoints.GetCreditablyEntityMaxPriority(platformName, creditableEntityName))
                .GetJsonAsync<int?>();
        }

        public override IFlurlRequest Request(params object[] urlSegments)
        {
            return base.Request(urlSegments)
                .WithHeader(ApiKeyHeader, _apiKey);
        }
    }
}
