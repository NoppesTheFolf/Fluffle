using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SixLabors.ImageSharp.Formats.Png;

namespace Noppes.Fluffle.Client;

internal class FluffleApiClient : IFluffleApiClient
{
    private readonly IFlurlClient _flurlClient;

    public FluffleApiClient(string baseUrl, string userAgent)
    {
        _flurlClient = new FlurlClient(baseUrl)
            .WithHeader("User-Agent", userAgent);

        _flurlClient.Settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[]
            {
                    new StringEnumConverter(new CamelCaseNamingStrategy())
            }
        });
    }

    public async Task<FluffleSearchResponse> ReverseSearchAsync(Stream file, bool? includeNsfw = null, IEnumerable<FlufflePlatform>? platforms = null, int? limit = null, bool? createLink = null)
    {
        using var sourceImage = await Image.LoadAsync(file);
        var targetWidth = sourceImage.Width < sourceImage.Height ? 256 : 0;
        var targetHeight = targetWidth == 0 ? 256 : 0;
        sourceImage.Mutate(x => x.Resize(targetWidth, targetHeight));

        using var destImage = new MemoryStream();
        await sourceImage.SaveAsync(destImage, new PngEncoder());
        destImage.Position = 0;

        try
        {
            var response = await _flurlClient.Request("/v1/search")
                .PostMultipartAsync(options =>
                {
                    options.AddFile("file", destImage, "dummy");

                    if (includeNsfw != null)
                        options.AddString("includeNsfw", includeNsfw.ToString());

                    foreach (var platform in platforms ?? Enum.GetValues<FlufflePlatform>())
                        options.AddString("platforms", platform.ToString());

                    if (limit != null)
                        options.AddString("limit", limit.ToString());

                    if (createLink != null)
                        options.AddString("createLink", createLink.ToString());
                }).ConfigureAwait(false);

            var body = await response.GetJsonAsync<FluffleSearchResponse>().ConfigureAwait(false);
            return body;
        }
        catch (FlurlHttpException e)
        {
            try
            {
                var error = await e.GetResponseJsonAsync<FluffleErrorResponse>().ConfigureAwait(false);
                throw new FluffleException(error);
            }
            catch
            {
                // ignored
            }

            throw;
        }
    }
}
