using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noppes.Fluffle.Utils
{
    /// <summary>
    /// For some bizarre reason, the built-in JSON serializer/deserializer doesn't support <see
    /// cref="TimeSpan"/>. This converter adds support for this type just as Newtonsoft.Json would
    /// handle the type.
    /// </summary>
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        /// <inheritdoc/>
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (value == null)
                throw new InvalidOperationException("Can't deserialize TimeSpan from null value.");

            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            var timespan = value.ToString(null, CultureInfo.InvariantCulture);

            writer.WriteStringValue(timespan);
        }
    }
}
