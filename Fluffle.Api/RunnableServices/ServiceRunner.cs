using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices;

/// <summary>
/// Base class for classes which are responsible for the execution of services. This base class
/// manages the interval at which services are executed and also makes sure that if a service
/// fails, for whatever reason, gets ran again.
/// </summary>
public abstract class ServiceRunner
{
    protected readonly IServiceProvider Services;
    protected readonly Type ServiceType;
    private readonly ILogger<ServiceRunner> _logger;
    private readonly TimeSpan _interval;
    private readonly CancellationToken _cancellationToken;
    private bool _isFirstRun;

    protected ServiceRunner(IServiceProvider services, Type serviceType, TimeSpan interval, CancellationToken cancellationToken)
    {
        Services = services;
        ServiceType = serviceType;
        _interval = interval;
        _cancellationToken = cancellationToken;
        _isFirstRun = true;
        _logger = services.GetRequiredService<ILogger<ServiceRunner>>();
    }

    public async Task RunAsync()
    {
        while (true)
        {
            try
            {
                if (!_isFirstRun)
                {
                    _logger.LogInformation("Waiting for {time} until running {service} again.",
                        _interval.Humanize(), ServiceType.Name.Humanize().ToLowerInvariant());
                    await Task.Delay(_interval, _cancellationToken);
                }

                var service = GetService();

                if (service is IInitializable initializable)
                    await InitializeAsync(initializable);

                await service.RunAsync();
                _isFirstRun = false;
            }
            catch (TaskCanceledException)
            {
                var service = GetService();

                if (service is IShutdownable shutdownable)
                    await shutdownable.ShutdownAsync();

                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "{service} threw an exception.", ServiceType.Name.Humanize());
            }
        }
    }

    protected abstract IService GetService();

    protected abstract Task InitializeAsync(IInitializable initializable);
}
