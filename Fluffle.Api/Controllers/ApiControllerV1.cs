using Microsoft.AspNetCore.Mvc;

namespace Noppes.Fluffle.Api.Controllers
{
    /// <summary>
    /// Base class for all version 1 API controllers. Works nicely with services due to the helper
    /// methods it has.
    /// </summary>
    [ApiVersion("1")]
    public abstract class ApiControllerV1 : ApiController
    {
    }
}
