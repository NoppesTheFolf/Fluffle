using Flurl.Http;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Client;

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
            .GetJsonExplicitlyAsync<SyncStateModel>();
    }

    public Task<ICollection<string>> SearchContentAsync(string platformName, SearchContentModel model)
    {
        return Request(Endpoints.SearchContent(platformName))
            .PostJsonReceiveJsonExplicitlyAsync<ICollection<string>>(model);
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
            .DeleteJsonReceiveJsonExplicitlyAsync<ICollection<int>>(model);
    }

    public Task<ICollection<string>> DeleteContentAsync(string platformName, IEnumerable<string> platformContentIds)
    {
        return Request(Endpoints.DeleteContent(platformName))
            .DeleteJsonReceiveJsonExplicitlyAsync<ICollection<string>>(platformContentIds);
    }

    public Task<PlatformModel> GetPlatformAsync(string name)
    {
        return Request(Endpoints.GetPlatform(name))
            .GetJsonExplicitlyAsync<PlatformModel>();
    }

    public Task SignalPlatformSyncAsync(string platformName, SyncTypeConstant syncType)
    {
        return Request(Endpoints.SignalPlatformSync(platformName, syncType))
            .PutAsync();
    }

    public Task<IList<PlatformModel>> GetPlatformsAsync()
    {
        return Request(Endpoints.GetPlatforms)
            .GetJsonExplicitlyAsync<IList<PlatformModel>>();
    }

    public Task<IList<UnprocessedImageModel>> GetUnprocessedImagesAsync(string platformName)
    {
        return Request(Endpoints.GetUnprocessedImages(platformName))
            .GetMessagePackExplicitlyAsync<IList<UnprocessedImageModel>>();
    }

    public Task<string> GetContentToRetryAsync(string platformName)
    {
        return Request(Endpoints.GetContentToRetry(platformName)).GetJsonExplicitlyAsync<string>();
    }

    public Task<IList<StatusModel>> GetStatusAsync()
    {
        return Request(Endpoints.GetStatus)
            .GetJsonExplicitlyAsync<IList<StatusModel>>();
    }

    public Task<int?> GetMinId(string platformName)
    {
        return Request(Endpoints.GetMinId(platformName))
            .GetJsonExplicitlyAsync<int?>();
    }

    public Task<int?> GetMaxId(string platformName)
    {
        return Request(Endpoints.GetMaxId(platformName))
            .GetJsonExplicitlyAsync<int?>();
    }

    public Task<PlatformSyncModel> GetPlatformSync(string platformName, SyncTypeConstant syncType)
    {
        return Request(Endpoints.GetPlatformSync(platformName, syncType))
            .GetJsonExplicitlyAsync<PlatformSyncModel>();
    }

    public Task<ImagesSyncModel> GetSyncImagesAsync(string platformName, long afterChangeId)
    {
        return Request(Endpoints.GetSyncImages(platformName, afterChangeId))
            .GetMessagePackExplicitlyAsync<ImagesSyncModel>();
    }

    public Task<CreditableEntitiesSyncModel> GetSyncCreditableEntitiesAsync(string platformName, long afterChangeId)
    {
        return Request(Endpoints.GetSyncCreditableEntities(platformName, afterChangeId))
            .GetMessagePackExplicitlyAsync<CreditableEntitiesSyncModel>();
    }

    public Task<ICollection<OtherSourceModel>> GetOtherSourcesAsync(int afterId)
    {
        return Request(Endpoints.GetOtherSources(afterId))
            .GetMessagePackExplicitlyAsync<ICollection<OtherSourceModel>>();
    }

    public Task<int?> GetCreditableEntitiesMaxPriority(string platformName, string creditableEntityName)
    {
        return Request(Endpoints.GetCreditablyEntityMaxPriority(platformName, creditableEntityName))
            .GetJsonExplicitlyAsync<int?>();
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments)
            .WithHeader(ApiKeyHeader, _apiKey);
    }
}
