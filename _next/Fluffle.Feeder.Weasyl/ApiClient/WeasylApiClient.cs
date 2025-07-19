using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Weasyl.ApiClient;

internal class WeasylApiClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public WeasylApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<int> GetNewestIdAsync()
    {
        var submissions = await GetFrontPageSubmissionsAsync();
        var newestId = submissions.Max(x => x.SubmitId)!.Value;

        return newestId;
    }

    public async Task<ICollection<WeasylFrontPageSubmission>> GetFrontPageSubmissionsAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(WeasylApiClient));

        const string url = "/api/submissions/frontpage";
        var submissions = await httpClient.GetFromJsonAsync<ICollection<WeasylFrontPageSubmission>>(url, JsonSerializerOptions);

        return submissions!;
    }

    public async Task<WeasylSubmission?> GetSubmissionAsync(int submissionId, bool anyway)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(WeasylApiClient));

        var url = $"/api/submissions/{submissionId}/view";

        if (anyway)
            url += "?anyway=true";

        using var response = await httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var submission = await response.Content.ReadFromJsonAsync<WeasylSubmission>(JsonSerializerOptions);
        return submission;
    }
}
