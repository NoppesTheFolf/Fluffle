﻿using FluentValidation;
using FluentValidation.AspNetCore;
using Humanizer;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Filters;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Telemetry;
using Noppes.Fluffle.Utils;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noppes.Fluffle.Api;

public abstract class ApiStartup<TConfiguration, TContext> : ApiStartup<TConfiguration> where TConfiguration : class where TContext : DbContext
{
    public override void BeforeConfigure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        using var context = scope.ServiceProvider.GetRequiredService<TContext>();
        context.Database.SetCommandTimeout(10.Minutes());

        logger.LogInformation("Applying migrations...");
        context.Database.Migrate();

        base.BeforeConfigure(app, env);
    }
}

public abstract class ApiStartup<TConfiguration> : ApiStartup where TConfiguration : class
{
    public override FluffleConfiguration GetConfiguration()
    {
        return FluffleConfiguration.Load<TConfiguration>(false);
    }
}

public abstract class ApiStartup
{
    private const string DevelopmentCorsPolicyName = "_development";
    private const string ProductionCorsPolicyName = "_production";

    protected abstract string ApplicationName { get; }

    /// <summary>
    /// Whether or not to enable access control through API keys and permissions.
    /// </summary>
    protected abstract bool EnableAccessControl { get; }

    protected FluffleConfiguration Configuration { get; }

    protected ApiStartup()
    {
        Configuration = GetConfiguration();
    }

    public abstract FluffleConfiguration GetConfiguration();

    public void ConfigureServices(IServiceCollection services)
    {
        BeforeConfigureServices(services);

        services.AddResponseCompression(options =>
        {
            // Add gzip compression for the default mime types and responses encoded using MessagePack
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/x-msgpack"
            }).ToList();
        });

        // Register all the mappers
        Mappers.Initialize();

        // Register add the services
        services.AddServices();

        // Add in-memory caching
        services.AddMemoryCache();

        services.AddCors(options =>
        {
            // Use a lax CORS policy in development to prevent issues where the browsers starts
            // whining about the used CORS policy
            options.AddPolicy(DevelopmentCorsPolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            options.AddPolicy(ProductionCorsPolicyName, builder =>
            {
                // TODO: This should probably be part of a configuration file
                builder.WithOrigins("https://www.fluffle.xyz", "https://fluffle.xyz").AllowAnyMethod();
            });
        });

        // ASP.NET Core will always run behind a reverse proxy, so we need to use those headers
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;

            // Only loopback proxies are allowed by default.
            // Clear that restriction because forwarders are enabled by explicit
            // configuration.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        if (EnableAccessControl)
        {
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            });

            ConfigureAuthentication(services, authenticationBuilder);
        }

        // We can't put this in the lambda expression passed to the AddControllers call because
        // it's called after the service provider has been built
        if (EnableAccessControl)
            services.AddTransient(s => s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User);

        services.AddControllers(options =>
        {
            options.Filters.Add<RequestExceptionFilter>();

            if (EnableAccessControl)
            {
                // Force all endpoints to be authenticated by default. Requires explicitly
                // telling ASP.NET that a endpoint can be accessed anonymously.
                var authenticationPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                var authorizeFilter = new AuthorizeFilter(authenticationPolicy);

                options.Filters.Add(authorizeFilter);
            }

            var messagePackOptions = ContractlessStandardResolver.Options.WithSecurity(MessagePackSecurity.UntrustedData);
            options.OutputFormatters.Add(new MessagePackOutputFormatter(messagePackOptions));
        }).ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Keys
                    .Zip(context.ModelState.Values, (key, value) => (k: key, e: value.Errors))
                    .Where(x => x.e.Any())
                    .ToDictionary(x => x.k.Camelize(), x => x.e.Select(e => e.ErrorMessage));

                return new BadRequestObjectResult(new V1ValidationError
                {
                    Code = "VALIDATION_FAILED",
                    Message = "One or more validation errors occurred.",
                    Errors = errors
                });
            };
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            AspNetJsonSerializer.Options = options.JsonSerializerOptions;
        }).AddFluentValidation(options =>
        {
            ValidatorOptions.Global.PropertyNameResolver = (_, member, _) => member?.Name;
            ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member?.Name;

            options.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            options.DisableDataAnnotationsValidation = true;
        });

        services.AddTelemetry(Configuration, ApplicationName);
        services.AddHostedService<TelemetryBufferFlusher>();

        services.AddHostedService<ServiceShutdownSignaler>();
        services.AddSingleton<ServiceBuilder>();

        services.AddApiVersioning();

        AdditionalConfigureServices(services);
    }

    public virtual void ConfigureAuthentication(IServiceCollection services, AuthenticationBuilder authenticationBuilder)
    {
    }

    public virtual void BeforeConfigureServices(IServiceCollection services)
    {
    }

    public virtual void AdditionalConfigureServices(IServiceCollection services)
    {
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var serviceBuilder = app.ApplicationServices.GetRequiredService<ServiceBuilder>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<ApiStartup>>();

        BeforeConfigure(app, env);

        app.UseResponseCompression();

        if (env.IsDevelopment())
        {
            logger.LogInformation("Using development CORS policy.");
            app.UseCors(DevelopmentCorsPolicyName);
        }

        if (env.IsProduction())
        {
            logger.LogInformation("Using production CORS policy.");
            app.UseCors(ProductionCorsPolicyName);
        }

        app.UseForwardedHeaders();

        app.UseRouting();

        if (EnableAccessControl)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            serviceBuilder.AddStartup<PermissionSeeder>();

            logger.LogInformation("Enabled access control.");
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        AfterConfigure(app, env, serviceBuilder);
        serviceBuilder.StartAsync().Wait();

        logger.LogInformation("The debug key for this session is {debugKey}.", DebugFilter.DebugKey);
    }

    public virtual void BeforeConfigure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }

    public virtual void AfterConfigure(IApplicationBuilder app, IWebHostEnvironment env, ServiceBuilder serviceBuilder)
    {
    }
}
