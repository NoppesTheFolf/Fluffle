using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.DeviantArt.Shared;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Service;

namespace Noppes.Fluffle.DeviantArt.DeviationsProcessor;

internal class Program : QueuePollingBatchService<Program, ProcessDeviationQueueItem>
{
    protected override TimeSpan Interval => _configuration.Interval;
    protected override TimeSpan VisibleAfter => _configuration.QueueMessagesVisibleAfter ?? TimeSpan.Zero;

    private readonly DeviationsProcessor _processor;
    private readonly DeviantArtDeviationsProcessorConfiguration _configuration;

    public Program(IServiceProvider services, DeviationsProcessor processor, DeviantArtDeviationsProcessorConfiguration configuration) : base(services)
    {
        _processor = processor;
        _configuration = configuration;
    }

    private static async Task Main(string[] args) => await RunAsync(args, "DeviantArtDeviationsProcessor", (conf, services) =>
    {
        services.AddDeviantArt(conf, x => x.DeviationsProcessor, true, true, true, false);

        var mainConf = conf.Get<MainConfiguration>();
        services.AddSingleton(new FluffleClient(mainConf.Url, mainConf.ApiKey));

        services.AddSingleton<DeviationsProcessor>();
        services.AddSingleton<DeviationsSubmitter>();
    });

    public override async Task ProcessAsync(ICollection<ProcessDeviationQueueItem> values, CancellationToken cancellationToken)
    {
        // It is possible for the queue to contains deviations with duplicate IDs, filter them out for the sake of efficiency
        var deviationIds = values.Select(x => x.Id).Distinct().ToList();

        await _processor.ProcessAsync(deviationIds);
    }
}
