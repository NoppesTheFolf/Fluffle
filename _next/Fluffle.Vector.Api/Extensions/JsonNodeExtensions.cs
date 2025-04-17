using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fluffle.Vector.Api.Extensions;

public static class JsonNodeExtensions
{
    public static ExpandoObject ToExpando(this JsonNode node)
    {
        if (node.GetValueKind() != JsonValueKind.Object)
            throw new ArgumentException("Node must be an object.", nameof(node));

        IDictionary<string, object?> dictionary = new ExpandoObject();
        foreach (var property in node.AsObject())
        {
            dictionary.Add(new KeyValuePair<string, object?>(property.Key, GetValue(property.Value!)));
        }

        return (dictionary as ExpandoObject)!;
    }

    private static object? GetValue(JsonNode node)
    {
        return node.GetValueKind() switch
        {
            JsonValueKind.Object => node.ToExpando(),
            JsonValueKind.Array => node.AsArray().Select(GetValue!).ToList(),
            JsonValueKind.String => node.GetValue<string>(),
            JsonValueKind.Number => node.GetValue<long>(),
            JsonValueKind.True => node.GetValue<bool>(),
            JsonValueKind.False => node.GetValue<bool>(),
            JsonValueKind.Null => null,
            _ => throw new InvalidOperationException()
        };
    }
}
