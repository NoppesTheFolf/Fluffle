using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Fluffle.Search.Api.OpenApi;

internal class FluffleDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Fluffle Search API";

        document.Servers ??= new List<OpenApiServer>();
        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = "https://api.fluffle.xyz/"
        });

        return Task.CompletedTask;
    }
}
