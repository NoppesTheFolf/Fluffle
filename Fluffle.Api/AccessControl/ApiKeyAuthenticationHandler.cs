using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.AccessControl;

/// <summary>
/// This class handles the authentication of API keys. Authorization is done with the <see cref="PermissionsAttribute"/>.
/// </summary>
public class ApiKeyAuthenticationHandler<TApiKey, TPermission, TApiKeyPermission>
    : AuthenticationHandler<ApiKeyAuthenticationOptions>
    where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>, new()
    where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>, new()
    where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>, new()
{
    private const string JsonContentType = "application/json";

    private V1Error _authenticationError;

    private readonly AccessManager<TApiKey, TPermission, TApiKeyPermission> _accessManager;

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, AccessManager<TApiKey, TPermission, TApiKeyPermission> accessManager)
        : base(options, logger, encoder)
    {
        _accessManager = accessManager;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the API key header is set
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKeyHeaderValues))
        {
            _authenticationError = AccessControlErrors.HeaderNotSet();
            return AuthenticateResult.NoResult();
        }

        // Check if there is actually a value in the set header
        var headerApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(headerApiKey))
        {
            _authenticationError = AccessControlErrors.HeaderWithoutValue();
            return AuthenticateResult.NoResult();
        }

        // Check if the provided API key exists
        var apiKey = await _accessManager.GetApiKeyAsync(headerApiKey, true);
        if (apiKey == null)
        {
            _authenticationError = AccessControlErrors.InvalidApiKey();
            return AuthenticateResult.NoResult();
        }

        // Add the permissions attached to the API key as claims
        var permissionClaims = apiKey.ApiKeyPermissions
            .Select(akp => akp.Permission)
            .Select(p => Permissions.ClaimPrefix + p.Name)
            .Select(p => new Claim(p, string.Empty));

        // Also add information about the API key itself
        var claims = new List<Claim>(permissionClaims)
        {
            new Claim(ApiKeyAuthenticationOptions.IdClaimType, apiKey.Id.ToString()),
            new Claim(ApiKeyAuthenticationOptions.ClaimType, apiKey.Key)
        };

        // ASP.NET Identity magic c:
        var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
        var identities = new List<ClaimsIdentity>
        {
            identity
        };
        var principal = new ClaimsPrincipal(identities);
        var ticket = new AuthenticationTicket(principal, Options.Scheme);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties) =>
        HandleAsync(_authenticationError);

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties) =>
        HandleAsync(AccessControlErrors.Forbidden());

    private Task HandleAsync(V1Error error)
    {
        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        Response.ContentType = JsonContentType;

        var jsonError = AspNetJsonSerializer.Serialize(error);

        return Response.WriteAsync(jsonError);
    }
}
