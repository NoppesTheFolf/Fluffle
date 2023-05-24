using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Immutable;
using System.Linq;

namespace Noppes.Fluffle.Api.AccessControl;

/// <summary>
/// Attribute which allows you to define a set of permissions which are required to access the
/// resource on which this attribute is applied. Will return a 403 Forbidden error is the user
/// doesn't have the provided permissions.
/// </summary>
public class PermissionsAttribute : ActionFilterAttribute
{
    private readonly ImmutableHashSet<string> _permissions;

    public PermissionsAttribute(params string[] permissions)
    {
        // A user's permissions are stored in their claims. These claims are prefixed and
        // therefore the permissions in our hashset need to be prefixed too
        _permissions = permissions
            .Select(p => Permissions.ClaimPrefix + p)
            .ToImmutableHashSet();
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var claims = ((ControllerBase)context.Controller).User.Claims
            .Select(c => c.Type);

        var hasAllPermissions = _permissions.IsSubsetOf(claims);

        if (!hasAllPermissions)
        {
            var error = AccessControlErrors.Forbidden();
            context.Result = new ObjectResult(error)
            {
                StatusCode = 403 // 403: Forbidden
            };
            return;
        }

        base.OnActionExecuting(context);
    }
}
