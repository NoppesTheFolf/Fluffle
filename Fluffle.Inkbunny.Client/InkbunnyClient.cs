using Flurl.Http;
using Flurl.Http.Configuration;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Inkbunny.Client.Models;

namespace Noppes.Fluffle.Inkbunny.Client;

public class InkbunnyClient : ApiClient, IInkbunnyClient
{
    private const string BaseUrl = "https://inkbunny.net";
    private static readonly TimeSpan SidExpirationTime = 1.Hours();

    private readonly string _username;
    private readonly string? _password;
    private readonly AsyncLock _sidLock;
    private string? _sid;
    private DateTime _sidRefreshedWhen;

    public InkbunnyClient(string username, string? password, string userAgent) : base(BaseUrl)
    {
        _username = username;
        _password = password;
        _sidLock = new AsyncLock();

        FlurlClient.WithHeader("User-Agent", userAgent);
        FlurlClient.Configure(settings =>
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
        });
    }

    public async Task<SubmissionsResponse> GetSubmissionsAsync(IEnumerable<string> ids)
    {
        await RefreshSidAsync();

        var request = Request("api_submissions.php")
            .SetQueryParam("submission_ids", string.Join(",", ids))
            .SetQueryParam("show_description", "yes");
        var response = await request.GetJsonExplicitlyAsync<SubmissionsResponse>();

        return response;
    }

    public async Task<SubmissionsResponse> SearchSubmissionsAsync(SubmissionSearchOrder order)
    {
        await RefreshSidAsync();

        var request = Request("api_search.php")
            .SetQueryParam("order", order.ToString().Underscore());
        var response = await request.GetJsonExplicitlyAsync<SubmissionsResponse>();

        return response;
    }

    public async Task<Login> LoginAsync(string username, string? password)
    {
        var request = Request("api_login.php")
            .SetQueryParam("username", username)
            .SetQueryParam("password", password);
        var response = await request.GetJsonExplicitlyAsync<Login>();

        return response;
    }

    private async Task RefreshSidAsync()
    {
        using var _ = await _sidLock.LockAsync();

        if (_sid == null || DateTime.UtcNow.Subtract(_sidRefreshedWhen) > SidExpirationTime)
        {
            var login = await LoginAsync(_username, _password);
            _sid = login.Sid;
            _sidRefreshedWhen = DateTime.UtcNow;
        }
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments).SetQueryParam("sid", _sid);
    }
}
