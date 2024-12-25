using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.LinkCreation;

public class LinkCreatorUpdater : Consumer<SearchRequest>
{
    private readonly IServiceProvider _services;

    public LinkCreatorUpdater(IServiceProvider services)
    {
        _services = services;
    }

    public override async Task<SearchRequest> ConsumeAsync(SearchRequest data)
    {
        using var _ = await LinkCreator.BeingProcessedLock.LockAsync();

        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<FluffleSearchContext>();

        var searchRequest = await context.SearchRequests.SingleAsync(sr => sr.Id == data.Id);
        searchRequest.LinkCreated = true;

        await context.SaveChangesAsync();
        LinkCreator.BeingProcessed.Remove(searchRequest.Id);

        return searchRequest;
    }
}
