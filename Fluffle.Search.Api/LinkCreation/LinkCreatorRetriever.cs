using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.LinkCreation;

public class LinkCreatorRetriever : Producer<SearchRequestV2>
{
    private const int BatchSize = 20;

    private readonly IServiceProvider _services;

    public LinkCreatorRetriever(IServiceProvider services)
    {
        _services = services;
    }

    public override async Task WorkAsync()
    {
        List<SearchRequestV2> searchRequests;
        using (var _ = await LinkCreator.BeingProcessedLock.LockAsync())
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<FluffleSearchContext>();

            searchRequests = await context.SearchRequestsV2
                .Where(sr => !LinkCreator.BeingProcessed.Contains(sr.Id) && sr.LinkCreated == false)
                .OrderByDescending(sr => sr.Id)
                .Take(BatchSize)
                .ToListAsync();
        }

        if (searchRequests.Count == 0)
        {
            await Task.Delay(60_000);
            return;
        }

        foreach (var searchRequest in searchRequests)
            await Enqueue(searchRequest);
    }

    public async Task Enqueue(SearchRequestV2 searchRequest)
    {
        using var _ = await LinkCreator.BeingProcessedLock.LockAsync();

        if (LinkCreator.BeingProcessed.Contains(searchRequest.Id))
            return;

        LinkCreator.BeingProcessed.Add(searchRequest.Id);
        await ProduceAsync(searchRequest);
    }
}
