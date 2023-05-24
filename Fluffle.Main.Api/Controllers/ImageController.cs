using Microsoft.AspNetCore.Mvc;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Main.Api.Services;
using Noppes.Fluffle.Main.Communication;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers;

public class ImageController : ApiControllerV1
{
    public const string Singular = PlatformController.SingularRoute + "/" + Endpoints.Image + "/{platformImageId}";
    public const string Plural = PlatformController.SingularRoute + "/" + Endpoints.Images;

    private readonly IContentService _contentService;
    private readonly IIndexService _indexService;

    public ImageController(IContentService contentService, IIndexService indexService)
    {
        _contentService = contentService;
        _indexService = indexService;
    }

    [HttpGet(Plural + "/unprocessed")]
    [Permissions(ContentPermissions.ReadUnprocessed)]
    public async Task<IActionResult> Unprocessed(string platformName)
    {
        var result = await _contentService.GetUnprocessedImages(platformName);

        // MessagePack can't handle enumerables, so we'll have to put everything into a list
        return HandleV1(result, data => Ok(data.ToList()));
    }

    [HttpPut(Singular + "/index")]
    [Permissions(IndexPermissions.Create, IndexPermissions.Update)]
    public async Task<IActionResult> PutIndex([FromRoute] string platformName, [FromRoute] string platformImageId, [FromBody] PutImageIndexModel model)
    {
        var result = await _indexService.Index(platformName, platformImageId, model);

        return HandleV1(result);
    }
}

public class IndexPermissions : Permissions
{
    public const string Prefix = "INDEX_";

    [Permission]
    public const string Create = Prefix + "CREATE";

    [Permission]
    public const string Update = Prefix + "UPDATE";
}
