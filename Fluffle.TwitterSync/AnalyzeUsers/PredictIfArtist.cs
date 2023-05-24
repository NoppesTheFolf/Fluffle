using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers;

public class PredictIfArtist : Consumer<AnalyzeUserData>
{
    private readonly IServiceProvider _services;
    private readonly IPredictionClient _predictionClient;

    public PredictIfArtist(IServiceProvider services, IPredictionClient predictionClient)
    {
        _services = services;
        _predictionClient = predictionClient;
    }

    public override async Task<AnalyzeUserData> ConsumeAsync(AnalyzeUserData data)
    {
        var artistIds = data.BestMatches
            .SelectMany(bestMatch => bestMatch == null || bestMatch.Credits.Count > 1
                ? Array.Empty<int>()
                : bestMatch.Credits.Select(c => c.Id).ToArray())
            .Distinct()
            .ToList();

        var isFurryArtist = await HttpResiliency.RunAsync(() => _predictionClient.IsFurryArtistAsync(data.Classes, artistIds));
        Log.Information("Is @{username} a furry artist? {value}", data.Username, isFurryArtist);

        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();
        var user = await context.Users.FirstAsync(u => u.Id == data.Id);

        user.IsFurryArtist = isFurryArtist;
        await context.SaveChangesAsync();

        return data;
    }
}
