namespace Noppes.Fluffle.Client;

/// <summary>
/// Builds an <see cref="IFluffleApiClient"/>.
/// </summary>
public class FluffleApiClientBuilder
{
    private const string DefaultBaseUrl = "https://api.fluffle.xyz";

    private string? _baseUrl;
    private string? _userAgent;

    /// <summary>
    /// Define the User-Agent to send when making a request. Setting this is required.
    /// </summary>
    public FluffleApiClientBuilder WithUserAgent(string appName, string appVersion, string username, string platform)
    {
        _userAgent = $"{appName}/{appVersion} (by {username} on {platform})";

        return this;
    }

    /// <summary>
    /// Define a custom base URL instead of the default one (https://api.fluffle.xyz).
    /// </summary>
    public FluffleApiClientBuilder WithBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;

        return this;
    }

    /// <summary>
    /// Build an instance of <see cref="IFluffleApiClient"/>.
    /// </summary>
    public IFluffleApiClient Build()
    {
        if (string.IsNullOrEmpty(_userAgent))
            throw new InvalidOperationException("No User-Agent specified. This is required for making requests to Fluffle.");

        return new FluffleApiClient(string.IsNullOrEmpty(_baseUrl) ? DefaultBaseUrl : _baseUrl, _userAgent);
    }
}
