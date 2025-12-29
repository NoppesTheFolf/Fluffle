using Fluffle.Imaging.Api.Models;
using System.Net;
using System.Net.Http.Json;

namespace Fluffle.Imaging.Api.Client;

internal static class HttpResponseMessageExtensions
{
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
                throw;
            }

            var errorModel = await httpResponseMessage.Content.ReadFromJsonAsync<ImagingErrorModel>(ImagingApiClient.JsonSerializerOptions);
            throw new ImagingApiException(errorModel!.Code, errorModel.Message);
        }
    }
}
