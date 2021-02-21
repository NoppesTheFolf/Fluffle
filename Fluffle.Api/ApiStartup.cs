using FluentValidation.AspNetCore;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Filters;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Noppes.Fluffle.Utils;

namespace Noppes.Fluffle.Api
{
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

            // Register all the mappers
            Mappers.Initialize();

            // Register add the services
            services.AddServices();

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
                options.ForwardedHeaders = ForwardedHeaders.All;
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
            {
                services.AddTransient(s => s
                    .GetRequiredService<IHttpContextAccessor>().HttpContext?.User);
            }

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
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = false;
                options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

                AspNetJsonSerializer.Options = options.JsonSerializerOptions;
            }).AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

                options.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            });

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
            var serviceManager = app.ApplicationServices.GetRequiredService<ServiceBuilder>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<ApiStartup>>();

            BeforeConfigure(app, env);

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

                serviceManager.AddStartup<PermissionSeeder>();

                logger.LogInformation("Enabled access control.");
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            AfterConfigure(app, env, serviceManager);
            serviceManager.StartAsync().Wait();
        }

        public virtual void BeforeConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        public virtual void AfterConfigure(IApplicationBuilder app, IWebHostEnvironment env, ServiceBuilder serviceBuilder)
        {
        }
    }
}
