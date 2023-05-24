using Humanizer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.LinkCreation;

public class LinkCreator : BackgroundService
{
    private static readonly TimeSpan CrashInterval = 30.Seconds();

    public static AsyncLock BeingProcessedLock { get; } = new();
    public static HashSet<string> BeingProcessed { get; } = new();

    private readonly IServiceProvider _services;
    private readonly ILogger<LinkCreator> _logger;

    public LinkCreator(IServiceProvider services, ILogger<LinkCreator> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                BeingProcessed.Clear();

                var manager = new ProducerConsumerManager<SearchRequestV2>(_services, 500);
                manager.AddProducer<LinkCreatorRetriever>(1);
                manager.AddConsumer<LinkCreatorUploader>(8, 50);
                manager.AddFinalConsumer<LinkCreatorUpdater>(1);
                await manager.RunAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(LinkCreator)} crashed! Waiting {{interval}} before retrying...", CrashInterval);
                await Task.Delay(CrashInterval);
            }
        }
    }
}
