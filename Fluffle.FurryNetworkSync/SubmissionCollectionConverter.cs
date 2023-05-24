using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.FurryNetworkSync;

public class SubmissionCollectionConverter : JsonConverter<ICollection<FnSubmission>>
{
    public override void WriteJson(JsonWriter writer, ICollection<FnSubmission> value, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override ICollection<FnSubmission> ReadJson(JsonReader reader, Type objectType, ICollection<FnSubmission> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.ReadFrom(reader);
        var submissions = token.Children();

        return submissions
            .Select(submission => submission["_source"])
            .Select(actualSubmission => serializer.Deserialize<FnSubmission>(actualSubmission.CreateReader()))
            .ToList();
    }
}
