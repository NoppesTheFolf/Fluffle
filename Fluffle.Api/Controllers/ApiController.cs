using Microsoft.AspNetCore.Mvc;
using Noppes.Fluffle.Api.Services;
using System;

namespace Noppes.Fluffle.Api.Controllers
{
    /// <summary>
    /// Base class for all API controllers. Supports API versioning. Works nicely with services due
    /// to the helper methods it has.
    /// </summary>
    [ApiController]
    [Route(BaseUrl)]
    public class ApiController : ControllerBase
    {
        public const string BaseUrl = "api/v{version:apiVersion}/";

        [NonAction]
        public IActionResult HandleV1<T>(SR<T> result, Func<T, IActionResult> onSuccess = null) where T : class
        {
            return result.Handle(HandleV1, r => onSuccess == null ? Ok(r) : onSuccess(r));
        }

        [NonAction]
        public IActionResult HandleV1(SE error)
        {
            return Handle(error, Ok, _ => new ObjectResult(new V1Error(error.Code, error.Message))
            {
                StatusCode = (int)error.HttpStatusCode
            });
        }

        [NonAction]
        public IActionResult Handle(SE error, Func<IActionResult> onSuccess, Func<SE, IActionResult> onError)
        {
            return error == null ? onSuccess() : onError(error);
        }
    }
}
