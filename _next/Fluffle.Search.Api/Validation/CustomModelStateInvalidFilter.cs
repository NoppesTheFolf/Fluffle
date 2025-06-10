using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Fluffle.Search.Api.Validation;

/// <summary>
/// Based on see <see cref="ModelStateInvalidFilter"/>, but with our own custom error model.
/// </summary>
public class CustomModelStateInvalidFilter : IActionFilter, IOrderedFilter
{
    public int Order => -2000;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        context.Result = Error.Create(400, context.ModelState);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
