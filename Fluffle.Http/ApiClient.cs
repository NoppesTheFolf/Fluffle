using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Noppes.Fluffle.Http;

/// <summary>
/// Simple base class for any API clients.
/// </summary>
public abstract class ApiClient : IDisposable
{
    private readonly List<ICallInterceptor> _interceptors;

    /// <summary>
    /// The rate limiter used on this HTTP client, might be null.
    /// </summary>
    public RequestRateLimiter RateLimiter { get; set; }

    /// <summary>
    /// The HTTP client used to make requests with.
    /// </summary>
    protected IFlurlClient FlurlClient { get; }

    protected ApiClient(string baseUrl)
    {
        // Enable compression if the handler supports it
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.AutomaticDecompression = httpClientHandler.SupportsAutomaticDecompression
            ? DecompressionMethods.All
            : DecompressionMethods.None;

        // Configure Flurl to make use of our custom http client and set the base URL
        var httpClient = new HttpClient(httpClientHandler);
        FlurlClient = new FlurlClient(httpClient);
        FlurlClient.BaseUrl = baseUrl;

        _interceptors = new List<ICallInterceptor>();
    }

    public void AddInterceptor<T>() where T : ICallInterceptor, new() => AddInterceptor(new T());

    public void AddInterceptor(ICallInterceptor interceptor) => _interceptors.Add(interceptor);

    /// <summary>
    /// Create a new request by combing the base url and provided url segments.
    /// </summary>
    public virtual IFlurlRequest Request(params object[] urlSegments)
    {
        var request = FlurlClient.Request(urlSegments);

        if (RateLimiter != null)
            request.AddInterceptor(RateLimiter);

        foreach (var interceptor in _interceptors)
            request.AddInterceptor(interceptor);

        return request;
    }

    public void Dispose()
    {
        FlurlClient.Dispose();

        GC.SuppressFinalize(this);
    }
}
