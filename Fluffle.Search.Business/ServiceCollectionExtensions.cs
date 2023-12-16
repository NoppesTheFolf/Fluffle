using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Search.Business.Similarity;

namespace Noppes.Fluffle.Search.Business;

public static class ServiceCollectionExtensions
{
    public static void AddBusiness(this IServiceCollection services)
    {
        services.AddSingleton<ISimilarityService, SimilarityService>();
    }
}
