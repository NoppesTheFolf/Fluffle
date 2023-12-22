using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Search.Business.Similarity;

namespace Noppes.Fluffle.Search.Business;

public static class ServiceCollectionExtensions
{
    public static void AddBusiness(this IServiceCollection services, string similarityDataDumpLocation)
    {
        services.AddSingleton<ISimilarityDataSerializer>(x => new FileSystemSimilarityDataSerializer(
            similarityDataDumpLocation, x.GetRequiredService<ILogger<FileSystemSimilarityDataSerializer>>()));
        services.AddSingleton<ISimilarityService, SimilarityService>();
    }
}
