using Microsoft.AspNetCore.Mvc;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Main.Api.Services;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class ContentController : ApiControllerV1
    {
        public const string SingularRoute = PlatformController.SingularRoute + "/" + Endpoints.Content + "/{platformContentId}";
        public const string PluralRoute = PlatformController.SingularRoute + "/" + Endpoints.Content;

        private readonly IContentService _contentService;

        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet(PluralRoute + "/max-id")]
        [Permissions(ContentPermissions.ReadMaxId)]
        public async Task<IActionResult> GetMaxId(string platformName)
        {
            var result = await _contentService.GetMaxIdOnPlatform(platformName);

            return HandleV1(result);
        }

        [HttpDelete(PluralRoute + "/range")]
        [Permissions(ContentPermissions.Delete)]
        public async Task<IActionResult> DeleteRange(string platformName, DeleteContentRangeModel model)
        {
            var result = await _contentService.MarkRangeForDeletionAsync(platformName, model);

            return HandleV1(result);
        }

        [HttpDelete(SingularRoute)]
        [Permissions(ContentPermissions.Delete)]
        public async Task<IActionResult> Delete(string platformName, string platformContentId)
        {
            var error = await _contentService.MarkForDeletionAsync(platformName, platformContentId);

            return HandleV1(error);
        }

        [HttpPut(PluralRoute)]
        [Permissions(ContentPermissions.Create, ContentPermissions.Update)]
        public async Task<IActionResult> PutContent(string platformName, IList<PutContentModel> contentModels)
        {
            var error = await _contentService.PutContentAsync(platformName, contentModels);

            return HandleV1(error);
        }

        [HttpPut(SingularRoute + "/warning")]
        [Permissions(ContentPermissions.AddWarning)]
        public async Task<IActionResult> PutWarning(string platformName, string platformContentId, PutWarningModel model)
        {
            var error = await _contentService.PutWarningAsync(platformName, platformContentId, model);

            return HandleV1(error);
        }

        [HttpPut(SingularRoute + "/error")]
        [Permissions(ContentPermissions.AddError)]
        public async Task<IActionResult> PutError(string platformName, string platformContentId, PutErrorModel model)
        {
            var error = await _contentService.PutErrorAsync(platformName, platformContentId, model);

            return HandleV1(error);
        }
    }

    public class ContentPermissions : Permissions
    {
        public const string Prefix = "CONTENT_";

        [Permission]
        public const string Create = Prefix + "CREATE";

        [Permission]
        public const string Update = Prefix + "UPDATE";

        [Permission]
        public const string Delete = Prefix + "DELETE";

        [Permission]
        public const string AddWarning = Prefix + "ADD_WARNING";

        [Permission]
        public const string AddError = Prefix + "ADD_ERROR";

        [Permission]
        public const string ReadUnprocessed = Prefix + "READ_UNPROCESSED";

        [Permission]
        public const string ReadMaxId = Prefix + "READ_MAX_ID";
    }
}
