namespace Fluffle.Vector.Api.Authentication;

public static class Configuration
{
    public static IServiceCollection AddApiKey(this IServiceCollection services)
    {
        services.AddOptions<ApiKeyOptions>()
            .BindConfiguration(ApiKeyOptions.ApiKey)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<ApiKeyMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseApiKey(this IApplicationBuilder app)
    {
        app.UseMiddleware<ApiKeyMiddleware>();

        return app;
    }
}
