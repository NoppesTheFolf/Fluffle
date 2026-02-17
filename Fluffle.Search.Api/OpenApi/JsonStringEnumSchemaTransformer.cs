using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Fluffle.Search.Api.OpenApi;

// https://github.com/dotnet/aspnetcore/issues/61303#issuecomment-2933870406
internal class JsonStringEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.String;
        schema.Enum ??= [];
        schema.Enum.Clear();

        var names = Enum.GetNames(type);
        foreach (var name in names)
        {
            schema.Enum.Add(JsonValue.Create(name.ToLowerInvariant()));
        }

        return Task.CompletedTask;
    }
}
