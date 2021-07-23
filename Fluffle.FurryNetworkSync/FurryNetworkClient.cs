using Flurl.Http;
using Flurl.Http.Configuration;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Noppes.Fluffle.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurryNetworkSync
{
    public class FurryNetworkClient : ApiClient
    {
        public static readonly int MaximumSubmissionsPerSearch = 72;

        private readonly string _userAgent;
        private readonly AsyncLock _authMutex;
        private string _bearerToken;
        private string _refreshToken;
        private DateTimeOffset _refreshAt;

        public FurryNetworkClient(string baseUrl, string refreshToken, string userAgent, TimeSpan? interval = null) : base(baseUrl)
        {
            _userAgent = userAgent;
            _refreshToken = refreshToken;
            _refreshAt = DateTimeOffset.MinValue;
            _authMutex = new AsyncLock();

            // Furry Network makes use of the snake casing (snake_casing) naming convention, so we
            // need to configure our JSON serializer to use the same convention. Submissions
            // collections are also parsed differently.
            FlurlClient.Configure(settings =>
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                };

                jsonSettings.Converters.Add(new SubmissionCollectionConverter());
                settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });

            AddInterceptor(new RequestRateLimiter(interval ?? 2.Seconds()));
        }

        public Task<FnSearchResult> SearchAsync(int from = 0, int? size = null)
        {
            size ??= MaximumSubmissionsPerSearch;

            return AuthorizedRequest(r =>
            {
                r.SetQueryParams(new
                {
                    from,
                    size
                });
            }, r => r.GetJsonAsync<FnSearchResult>(), "api/search/artwork");
        }

        public Task<Stream> GetStreamAsync(string url)
        {
            return AuthorizedRequest(_ => { }, r => r.GetStreamAsync(), url);
        }

        public async Task<T> AuthorizedRequest<T>(Action<IFlurlRequest> buildRequest, Func<IFlurlRequest, Task<T>> makeRequest, params object[] urlSegments)
        {
            await RefreshTokenAsync();

            IFlurlRequest BuildAuthorizedRequest()
            {
                var request = Request(urlSegments)
                    .WithOAuthBearerToken(_bearerToken);

                buildRequest(request);

                return request;
            }

            try
            {
                return await makeRequest(BuildAuthorizedRequest());
            }
            catch (FlurlHttpException httpException)
            {
                if (httpException.StatusCode != 401)
                    throw;

                await RefreshTokenAsync(true);
                return await makeRequest(BuildAuthorizedRequest());

            }
        }

        public override IFlurlRequest Request(params object[] urlSegments)
        {
            return base.Request(urlSegments)
                .WithHeader("User-Agent", _userAgent);
        }

        private async Task RefreshTokenAsync(bool force = false)
        {
            using var _ = await _authMutex.LockAsync();

            if (_bearerToken != null && !force && DateTimeOffset.UtcNow < _refreshAt)
                return;

            try
            {
                var token = await GetTokenAsync();
                _bearerToken = token.AccessToken;
                // Multiply the expiration time by 0.9 to force the token to refresh before expiring
                _refreshAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresInSeconds * 0.9);
            }
            catch (FlurlHttpException e)
            {
                if (e.StatusCode == 400)
                    throw new InvalidOperationException("Authorization refresh token has expired.");

                throw;
            }
        }

        private Task<FnToken> GetTokenAsync()
        {
            return Request("api/oauth/token")
                .PostContentReceiveJsonAsync<FnToken>(new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", "123"), // No idea what this is, but this is what the browser sends to the server as well
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", _refreshToken)
                }));
        }
    }
}
