using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Inkbunny.Client.Converters;

internal class InkbunnyDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()!;
        value += ":00";

        var result = DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
