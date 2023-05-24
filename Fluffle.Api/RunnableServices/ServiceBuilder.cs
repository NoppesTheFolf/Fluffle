using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices;

/// <summary>
/// Kind of like a builder for <see cref="IService"/> instances, but not really. Three different
/// types of services can be added to this builder: startup, transient and singleton.
/// <para>
/// Startup services get created on the call to <see cref="StartAsync"/>, then their <see
/// cref="IService.RunAsync"/> method gets called immediately in a blocking fashion.
/// </para>
/// <para>
/// Singleton and transient services work similarly with the main difference being that
/// singleton instances get re-used, while transient instances do not. These types of services
/// can be run at a given interval.
/// </para>
/// </summary>
public class ServiceBuilder
{
    private class ServiceInfo
    {
        public Type Type { get; set; }

        public TimeSpan Interval { get; set; }

        public bool IsSingleton { get; set; }
    }

    private static readonly ManualResetEventSlim ShutdownEvent = new();
    private static readonly ICollection<ServiceBuilder> ServiceBuilders = new List<ServiceBuilder>();

    private readonly IServiceProvider _services;
    private readonly ICollection<ServiceInfo> _serviceInfos;
    private readonly ICollection<Type> _startupServiceTypes;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ICollection<(ServiceRunner runner, Task task)> _serviceRunners;

    public ServiceBuilder(IServiceProvider services)
    {
        _services = services;
        _serviceInfos = new List<ServiceInfo>();
        _startupServiceTypes = new List<Type>();
        _cancellationTokenSource = new CancellationTokenSource();
        _serviceRunners = new List<(ServiceRunner runner, Task task)>();

        ServiceBuilders.Add(this);
    }

    /// <summary>
    /// Add a singleton service which run at a given interval.
    /// </summary>
    public void AddSingleton<TService>(TimeSpan interval) where TService : IService =>
        Add<TService>(interval, true);

    /// <summary>
    /// Add a transient service which run at a given interval.
    /// </summary>
    public void AddTransient<TService>(TimeSpan interval) where TService : IService =>
        Add<TService>(interval, false);

    private void Add<TService>(TimeSpan interval, bool isSingleton) where TService : IService
    {
        _serviceInfos.Add(new ServiceInfo
        {
            Type = typeof(TService),
            Interval = interval,
            IsSingleton = isSingleton
        });
    }

    /// <summary>
    /// Adds a service which is ran only once in a blocking fashion on the call to <see cref="StartAsync"/>.
    /// </summary>
    public void AddStartup<TService>() where TService : IService =>
        _startupServiceTypes.Add(typeof(TService));

    public async Task StartAsync()
    {
        foreach (var startupType in _startupServiceTypes)
        {
            using var scope = _services.CreateScope();
            var singleRunService = (IService)scope.ServiceProvider.GetRequiredService(startupType);
            await singleRunService.RunAsync();
        }

        var singletonInfos = _serviceInfos
            .Where(i => i.IsSingleton)
            .ToList();

        foreach (var singleton in singletonInfos)
        {
            var runner = new SingletonServiceRunner(_services, singleton.Type, singleton.Interval, _cancellationTokenSource.Token);
            _serviceRunners.Add((runner, Task.Run(runner.RunAsync)));
        }

        var transientTypes = _serviceInfos
            .Except(singletonInfos);

        foreach (var transient in transientTypes)
        {
            var runner = new TransientServiceRunner(_services, transient.Type, transient.Interval, _cancellationTokenSource.Token);
            _serviceRunners.Add((runner, Task.Run(runner.RunAsync)));
        }
    }

    public static async Task Shutdown()
    {
        foreach (var serviceBuilder in ServiceBuilders)
            await serviceBuilder.StopAsync();
    }

    private async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();

        foreach (var startupType in _startupServiceTypes.Where(t => typeof(IShutdownable).IsAssignableFrom(t)))
        {
            using var scope = _services.CreateScope();
            var singleRunService = (IShutdownable)scope.ServiceProvider.GetRequiredService(startupType);
            await singleRunService.ShutdownAsync();
        }

        var tasks = _serviceRunners.Select(x => x.task).ToArray();
        await Task.WhenAll(tasks);
    }
}
