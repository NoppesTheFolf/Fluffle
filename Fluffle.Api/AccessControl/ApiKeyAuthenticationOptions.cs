using Microsoft.AspNetCore.Authentication;

namespace Noppes.Fluffle.Api.AccessControl;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string ClaimType = "API_KEY";
    public const string IdClaimType = "API_KEY_ID";
    public const string HeaderName = "Api-Key";

    public const string DefaultScheme = "API key";
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}
