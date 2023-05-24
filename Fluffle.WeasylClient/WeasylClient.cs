using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Weasyl.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Weasyl;

public class WeasylClient : ApiClient
{
    private readonly string _userAgent;
    private readonly string _apiKey;

    public WeasylClient(string baseUrl, string userAgent, string apiKey) : base(baseUrl)
    {
        _userAgent = userAgent;
        _apiKey = apiKey;

        FlurlClient.Settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        });
    }

    public async Task<IList<FontPageSubmission>> GetFrontPageAsync()
    {
        return await Request(true, "api", "submissions", "frontpage")
            .GetJsonExplicitlyAsync<IList<FontPageSubmission>>();
    }

    public async Task<Submission> GetSubmissionAsync(int id)
    {
        try
        {
            return await Request(true, "api", "submissions", id, "view")
                .GetJsonExplicitlyAsync<Submission>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode == (int)HttpStatusCode.NotFound)
                return null;

            throw;
        }
    }

    public Task<Stream> GetStreamAsync(string url) => Request(false, url).GetStreamAsync();

    public IFlurlRequest Request(bool isAuthenticated, params object[] urlSegments)
    {
        var request = base.Request(urlSegments).WithHeader("User-Agent", _userAgent);

        return isAuthenticated
            ? request.WithHeader("X-Weasyl-API-Key", _apiKey)
            : request;
    }
}
