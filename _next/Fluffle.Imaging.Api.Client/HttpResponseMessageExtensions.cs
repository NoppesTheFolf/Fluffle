using Fluffle.Imaging.Api.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fluffle.Imaging.Api.Client;

internal static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static async Task EnsureSuccessAsync(this HttpResponseMessage httpResponseMessage)
    {
        try
        {
            httpResponseMessage.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode != HttpStatusCode.BadRequest)
            {
                return;
            }

            var errorModel = await httpResponseMessage.Content.ReadFromJsonAsync<ImagingErrorModel>(JsonSerializerOptions);
            throw new ImagingApiException(errorModel!.Code, errorModel.Message);
        }
    }
}
