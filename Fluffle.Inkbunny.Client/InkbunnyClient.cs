using Flurl.Http;
using Flurl.Http.Configuration;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Inkbunny.Client.Models;

namespace Noppes.Fluffle.Inkbunny.Client;

public class InkbunnyClient : ApiClient, IInkbunnyClient
{
    private const string BaseUrl = "https://inkbunny.net";

    private readonly string _username;
    private readonly string? _password;
    private string? _sid;

    public InkbunnyClient(string username, string? password, string userAgent) : base(BaseUrl)
    {
        _username = username;
        _password = password;

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
        await RefreshSid();

        var request = Request("api_submissions.php")
            .SetQueryParam("submission_ids", string.Join(",", ids))
            .SetQueryParam("show_description", "yes");
        var response = await request.GetJsonAsync<SubmissionsResponse>();

        return response;
    }

    public async Task<SubmissionsResponse> SearchSubmissionsAsync(SubmissionSearchOrder order)
    {
        await RefreshSid();

        var request = Request("api_search.php")
            .SetQueryParam("order", order.ToString().Underscore());
        var response = await request.GetJsonAsync<SubmissionsResponse>();

        return response;
    }

    public async Task<Login> LoginAsync(string username, string? password)
    {
        var request = Request("api_login.php")
            .SetQueryParam("username", username)
            .SetQueryParam("password", password);
        var response = await request.GetJsonAsync<Login>();

        return response;
    }

    private async Task RefreshSid()
    {
        if (_sid != null)
            return;

        var login = await LoginAsync(_username, _password);
        _sid = login.Sid;
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments).SetQueryParam("sid", _sid);
    }
}
