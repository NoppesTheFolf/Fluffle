using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Telemetry;
using Serilog;
using SerilogTimings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service;

public abstract class Service : BackgroundService
{
    public IServiceProvider Services { get; internal set; }
    public IHostEnvironment Environment { get; }

    private readonly IHostApplicationLifetime _lifetime;
    private readonly ITelemetryClient _telemetryClient;

    protected Service(IServiceProvider services)
    {
        Services = services;
        Environment = services.GetRequiredService<IHostEnvironment>();
        _lifetime = services.GetRequiredService<IHostApplicationLifetime>();

        var telemetryClientFactory = services.GetRequiredService<ITelemetryClientFactory>();
        _telemetryClient = telemetryClientFactory.Create(nameof(Service));
    }

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ExecuteServiceAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "The implementation of the service threw an exception. This probably indicates there is a bug.");

            using (var _ = Operation.Time("Tracking exception in Application Insights"))
                await _telemetryClient.TrackExceptionAsync(exception);

            _lifetime.StopApplication();
        }
    }

    protected abstract Task ExecuteServiceAsync(CancellationToken stoppingToken);

    public sealed override async Task StopAsync(CancellationToken cancellationToken)
    {
        using (var _ = Operation.Time("Flushing Application Insights client buffer"))
            await _telemetryClient.FlushAsync();

        await base.StopAsync(cancellationToken);
    }
}

public abstract class Service<TService> : Service where TService : Service
{
    protected Service(IServiceProvider services) : base(services)
    {
    }

    public static async Task RunAsync<TCommandLineOptions>(string[] args, string applicationName, Action<TCommandLineOptions, FluffleConfiguration, IServiceCollection> configureServices = null)
    {
        var commandLineParseResult = new Parser(x => x.CaseInsensitiveEnumValues = true).ParseArguments<TCommandLineOptions>(args);
        if (commandLineParseResult.Errors.Any())
        {
            Log.Fatal("Parsing command line arguments failed.");
            System.Environment.Exit(-1);
        }

        await RunAsync(args, applicationName, (configuration, services) =>
        {
            configureServices?.Invoke(commandLineParseResult.Value, configuration, services);
        });
    }

    public static async Task RunAsync(string[] args, string applicationName, Action<FluffleConfiguration, IServiceCollection> configureServices = null)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args);

        hostBuilder.ConfigureServices(serviceCollection =>
        {
            serviceCollection.AddHostedService<TService>();

            var configuration = FluffleConfiguration.Load<TService>();
            serviceCollection.AddSingleton(configuration);

            serviceCollection.AddTelemetry(configuration, applicationName);

            configureServices?.Invoke(configuration, serviceCollection);
        }).UseSerilog();

        if (Debugger.IsAttached)
            hostBuilder.UseEnvironment("Development");

        await hostBuilder.Build().RunAsync();
    }
}
