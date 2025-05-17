namespace Fluffle.Vector.Api.Client;

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
            var bodyContent = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new VectorApiException(bodyContent, e);
        }
    }
}
