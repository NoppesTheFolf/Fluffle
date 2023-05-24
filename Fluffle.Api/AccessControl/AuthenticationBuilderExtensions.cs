using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Noppes.Fluffle.Api.AccessControl;

/// <summary>
/// Setting up API key access control requires the configuration of some services. This class
/// handles that.
/// </summary>
public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Add API key access control using the concrete types defined using generics.
    /// </summary>
    public static AuthenticationBuilder AddApiKeySupport<TContext, TApiKey, TPermission, TApiKeyPermission>(this AuthenticationBuilder authenticationBuilder,
        Action<ApiKeyAuthenticationOptions> options, IServiceCollection services)
        where TContext : ApiKeyContext<TApiKey, TPermission, TApiKeyPermission>
        where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>, new()
        where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>, new()
        where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>, new()
    {
        services.AddScoped<ApiKeyContext<TApiKey, TPermission, TApiKeyPermission>, TContext>();
        services.AddScoped<AccessManager<TApiKey, TPermission, TApiKeyPermission>>();
        services.AddSingleton<PermissionSeeder, PermissionSeeder<TApiKey, TPermission, TApiKeyPermission>>();

        return authenticationBuilder
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler<TApiKey, TPermission, TApiKeyPermission>>(
                ApiKeyAuthenticationOptions.DefaultScheme, options);
    }
}
