using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public static class ContentError
    {
        private const string NotFoundCode = "CONTENT_NOT_FOUND";

        public static SE ContentNotFound(string platformName, string platformContentId)
        {
            return new(NotFoundCode, HttpStatusCode.NotFound,
                $"No content exists for {platformName} with content ID `{platformContentId}`.");
        }
    }

    public interface IContentService
    {
        public Task<SE> MarkForDeletionAsync(string platformName, string idOnPlatform, bool saveChanges = true);

        public Task<SR<IEnumerable<int>>> MarkRangeForDeletionAsync(string platformName, DeleteContentRangeModel model);

        public Task<SE> DeleteAsync(string platformName, string idOnPlatform);

        public Task<SE> PutWarningAsync(string platformName, string platformContentId, PutWarningModel model);

        public Task<SE> PutErrorAsync(string platformName, string platformContentId, PutErrorModel model);

        public Task<SE> PutContentAsync(string platformName, IList<PutContentModel> contentModels);

        public Task<SR<IEnumerable<UnprocessedImageModel>>> GetUnprocessedImages(string platformName);

        public Task<SR<int?>> GetMinIdOnPlatform(string platformName);

        public Task<SR<int?>> GetMaxIdOnPlatform(string platformName);
    }
}
