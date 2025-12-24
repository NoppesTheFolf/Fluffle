using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.ReverseSearch.Api;

public class FluffleApiClient
{
    internal static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public FluffleApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<FluffleApiResponse> ExactSearchAsync(Stream stream, int limit)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(FluffleApiClient));

        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", "file");
        content.Add(new StringContent(limit.ToString(CultureInfo.InvariantCulture)), "limit");

        using var response = await httpClient.PostAsync("/exact-search-by-file", content);
        response.EnsureSuccessStatusCode();
        var apiResponse = await response.Content.ReadFromJsonAsync<FluffleApiResponse>(JsonSerializerOptions);

        apiResponse.Results = apiResponse.Results
            .Where(x => x.Platform != FluffleApiPlatform.Unknown)
            .ToList();

        return apiResponse;
    }
}
