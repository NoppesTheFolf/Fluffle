using Flurl.Http;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
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

        public Task<bool> GetFaBotsAllowed()
        {
            return Request(Endpoints.GetFaBotsAllowed())
                .GetJsonAsync<bool>();
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

        public Task DeleteContentAsync(string platformName, string platformContentId)
        {
            return Request(Endpoints.DeleteContent(platformName, platformContentId))
                .DeleteAsync();
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

        public Task<ImagesSyncModel> GetSyncImagesAsync(long afterChangeId)
        {
            return Request(Endpoints.GetSyncImages(afterChangeId))
                .GetMessagePackAsync<ImagesSyncModel>();
        }

        public Task<CreditableEntitiesSyncModel> GetSyncCreditableEntitiesAsync(long afterChangeId)
        {
            return Request(Endpoints.GetSyncCreditableEntities(afterChangeId))
                .GetMessagePackAsync<CreditableEntitiesSyncModel>();
        }

        public override IFlurlRequest Request(params object[] urlSegments)
        {
            return base.Request(urlSegments)
                .WithHeader(ApiKeyHeader, _apiKey);
        }
    }
}
