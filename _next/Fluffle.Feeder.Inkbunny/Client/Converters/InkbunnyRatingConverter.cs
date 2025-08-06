using Fluffle.Feeder.Inkbunny.Client.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Inkbunny.Client.Converters;

internal class InkbunnyRatingConverter : JsonConverter<InkbunnySubmissionRating>
{
    public override InkbunnySubmissionRating Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueStr = reader.GetString()!;
        var valueInt = int.Parse(valueStr);
        var value = (InkbunnySubmissionRating)valueInt;

        return value;
    }

    public override void Write(Utf8JsonWriter writer, InkbunnySubmissionRating value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
