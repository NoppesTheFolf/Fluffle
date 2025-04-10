using System.Dynamic;
using System.Text.Json;

namespace Fluffle.Ingestion.Api.Extensions;

public static class JsonElementExtensions
{
    public static ExpandoObject ToExpando(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Element must be an object.", nameof(element));

        IDictionary<string, object?> dictionary = new ExpandoObject();
        foreach (var property in element.EnumerateObject())
        {
            dictionary.Add(new KeyValuePair<string, object?>(property.Name, GetValue(property.Value)));
        }

        return (dictionary as ExpandoObject)!;
    }

    private static object? GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.ToExpando(),
            JsonValueKind.Array => element.EnumerateArray().Select(GetValue).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var value) ? value : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new InvalidOperationException()
        };
    }
}
