using Fluffle.Feeder.Inkbunny.Client.Models;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fluffle.Feeder.Inkbunny.Client;

internal class InkbunnyClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static readonly TimeSpan SidExpirationTime = TimeSpan.FromHours(1);
    private readonly AsyncLock _sidRefreshLock = new();
    private string? _sid;
    private DateTime _sidRefreshedWhen = DateTime.MinValue;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<InkbunnyClientOptions> _options;

    public InkbunnyClient(IHttpClientFactory httpClientFactory, IOptions<InkbunnyClientOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<InkbunnySubmissionsResponse> GetSubmissionsAsync(IEnumerable<string> ids)
    {
        var sid = await GetSidAsync();

        using var httpClient = _httpClientFactory.CreateClient(nameof(InkbunnyClient));

        var url = $"api_submissions.php?sid={Uri.EscapeDataString(sid)}&submission_ids={Uri.EscapeDataString(string.Join(",", ids))}";
        var response = await httpClient.GetFromJsonAsync<InkbunnySubmissionsResponse>(url, JsonSerializerOptions);

        return response!;
    }

    public async Task<InkbunnySearchResponse> SearchSubmissionsAsync(string order = "create_datetime")
    {
        var sid = await GetSidAsync();

        using var httpClient = _httpClientFactory.CreateClient(nameof(InkbunnyClient));

        var url = $"api_search.php?sid={Uri.EscapeDataString(sid)}&order={Uri.EscapeDataString(order)}";
        var response = await httpClient.GetFromJsonAsync<InkbunnySearchResponse>(url, JsonSerializerOptions);

        return response!;
    }

    private async Task<string> GetSidAsync()
    {
        using var _ = await _sidRefreshLock.LockAsync();

        if (_sid == null || DateTime.UtcNow.Subtract(_sidRefreshedWhen) > SidExpirationTime)
        {
            var login = await LoginAsync(_options.Value.Username, _options.Value.Password);

            _sid = login.Sid;
            _sidRefreshedWhen = DateTime.UtcNow;
        }

        return _sid;
    }

    private async Task<InkbunnyLogin> LoginAsync(string username, string password)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(InkbunnyClient));

        var url = $"/api_login.php?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
        var login = await httpClient.GetFromJsonAsync<InkbunnyLogin>(url, JsonSerializerOptions);

        return login!;
    }
}
