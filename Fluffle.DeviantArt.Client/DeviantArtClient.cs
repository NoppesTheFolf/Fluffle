﻿using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Noppes.Fluffle.DeviantArt.Client.Models;
using Noppes.Fluffle.Http;

namespace Noppes.Fluffle.DeviantArt.Client;

public class DeviantArtClient : ApiClient
{
    private readonly string _clientId;
    private readonly string _clientSecret;

    private readonly AsyncLock _authorizationLock;
    private DateTime? _accessTokenRetrievedAt;
    private string? _accessToken;

    public DeviantArtClient(string clientId, string clientSecret, string userAgent, string baseUrl = "https://www.deviantart.com") : base(baseUrl)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;

        _authorizationLock = new AsyncLock();
        _accessTokenRetrievedAt = null;
        _accessToken = null;

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

    public async Task<DeviantArtResponse<User?, UserError?>> GetProfileAsync(string username)
    {
        var request = (await AuthenticatedRequest("/api/v1/oauth2/user/profile", username))
            .SetQueryParam("expand", "user.details")
            .SetQueryParam("mature_content", true);

        var result = await MakeRequestAsync<User?, UserError?>(async () =>
        {
            var response = await request.GetJsonExplicitlyAsync<UserResponse>();

            return response.User;
        });

        return result;
    }

    public async Task<BrowseDeviationsResponse> BrowseGalleryAsync(string folderId, string username, int? offset = null, int? limit = null)
    {
        var request = (await AuthenticatedRequest("/api/v1/oauth2/gallery", folderId))
            .SetQueryParam("folderid", folderId)
            .SetQueryParam("username", username)
            .SetQueryParam("mode", "newest")
            .SetQueryParam("offset", offset)
            .SetQueryParam("limit", limit)
            .SetQueryParam("mature_content", true);

        var response = await request.GetJsonExplicitlyAsync<BrowseDeviationsResponse>();
        return response;
    }

    public IAsyncEnumerable<Deviation> EnumerateBrowseGalleryAsync(string folderId, string username) =>
        EnumerateDeviationsAsync(async offset => await BrowseGalleryAsync(folderId, username, offset, 24));

    public async Task<BrowseDeviationsResponse> BrowseNewestAsync(string? q = null, int? offset = null, int? limit = null)
    {
        var request = (await AuthenticatedRequest("/api/v1/oauth2/browse/newest"))
            .SetQueryParam("q", q)
            .SetQueryParam("offset", offset)
            .SetQueryParam("limit", limit)
            .SetQueryParam("mature_content", true);

        var response = await request.GetJsonExplicitlyAsync<BrowseDeviationsResponse>();
        return response;
    }

    public IAsyncEnumerable<Deviation> EnumerateBrowseNewestAsync(string? q = null) =>
        EnumerateDeviationsAsync(async offset => await BrowseNewestAsync(q, offset, 120));

    public async Task<DeviantArtResponse<Deviation?, DeviationError?>> GetDeviationAsync(string id)
    {
        var request = await AuthenticatedRequest("/api/v1/oauth2/deviation", id);
        var response = await MakeRequestAsync<Deviation?, DeviationError?>(async () => await request.GetJsonExplicitlyAsync<Deviation>());

        return response;
    }

    private static async Task<DeviantArtResponse<T, TError>> MakeRequestAsync<T, TError>(Func<Task<T>> makeRequestAsync)
    {
        try
        {
            var response = await makeRequestAsync();
            return new DeviantArtResponse<T, TError>(response);
        }
        catch (FlurlHttpException httpException)
        {
            if (httpException.Call.Response == null || httpException.StatusCode != 400)
                throw;

            var errorResponse = await httpException.Call.Response.GetJsonAsync<Error>();
            if (errorResponse.Code == null)
                throw;

            var enumType = typeof(TError).GenericTypeArguments.Single();
            var error = (TError)Enum.ToObject(enumType, errorResponse.Code);

            return new DeviantArtResponse<T, TError>(error);
        }
    }

    public async Task<IDictionary<string, DeviationMetadata>> GetDeviationMetadataAsync(IEnumerable<string> ids)
    {
        var metadata = new List<DeviationMetadata>();
        foreach (var chunk in ids.Distinct(StringComparer.OrdinalIgnoreCase).Chunk(10))
        {
            var request = (await AuthenticatedRequest("/api/v1/oauth2/deviation/metadata"))
                .SetQueryParam("deviationids[]", chunk)
                .SetQueryParam("ext_stats", true);

            var response = await request.GetJsonExplicitlyAsync<DeviationMetadataResponse>();
            metadata.AddRange(response.Metadata);
        }

        var result = metadata.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        return result;
    }

    private static async IAsyncEnumerable<Deviation> EnumerateDeviationsAsync(Func<int, Task<PaginatedResponse<Deviation>>> nextAsync)
    {
        var seenIds = new HashSet<string>();

        var offset = 0;
        bool hasMore;
        do
        {
            var response = await nextAsync(offset);
            foreach (var result in response.Results.Where(x => !seenIds.Contains(x.Id)).OrderByDescending(x => x.PublishedTime))
                yield return result;

            foreach (var result in response.Results)
                seenIds.Add(result.Id);

            if (response.HasMore)
                offset = (int)response.NextOffset!;

            hasMore = response.HasMore;
        } while (hasMore);
    }

    private async Task<IFlurlRequest> AuthenticatedRequest(params object[] urlSegments)
    {
        using var _ = await _authorizationLock.LockAsync();

        if (_accessTokenRetrievedAt == null || DateTime.UtcNow.Subtract((DateTime)_accessTokenRetrievedAt) > TimeSpan.FromMinutes(30))
            await RefreshAuthorizationAsync();

        return Request(urlSegments)
            .SetQueryParam("access_token", _accessToken);
    }

    private async Task RefreshAuthorizationAsync()
    {
        var request = Request("/oauth2/token")
            .SetQueryParam("grant_type", "client_credentials")
            .SetQueryParam("client_id", _clientId)
            .SetQueryParam("client_secret", _clientSecret);

        var response = await request.GetJsonExplicitlyAsync<TokenResponse>();
        _accessToken = response.AccessToken;
        _accessTokenRetrievedAt = DateTime.UtcNow;
    }
}