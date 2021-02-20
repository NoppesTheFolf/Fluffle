using System.Security.Claims;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Extensions to get API key information from a <see cref="ClaimsPrincipal"/> instance.
    /// </summary>
    public static class ApiKeyClaimExtensions
    {
        public static string GetApiKey(this ClaimsPrincipal claims)
        {
            return claims.FindFirstValue(ApiKeyAuthenticationOptions.ClaimType);
        }

        public static int GetApiKeyId(this ClaimsPrincipal claims)
        {
            return int.Parse(claims.FindFirstValue(ApiKeyAuthenticationOptions.IdClaimType));
        }
    }
}
