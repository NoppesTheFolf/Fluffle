using Flurl.Http;
using Polly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http;

public class FlurlRetryPolicyBuilder
{
    private readonly HashSet<int> _statusCodes = new();
    private bool _shouldRetryClientTimeouts;
    private bool _shouldRetryNetworkErrors;
    private (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? _retryConfiguration;

    public FlurlRetryPolicyBuilder WithStatusCode(HttpStatusCode statusCode) => WithStatusCode((int)statusCode);

    public FlurlRetryPolicyBuilder WithStatusCode(int statusCode)
    {
        _statusCodes.Add(statusCode);

        return this;
    }

    public FlurlRetryPolicyBuilder ShouldRetryClientTimeouts(bool value)
    {
        if (_shouldRetryClientTimeouts)
            _shouldRetryClientTimeouts = value;

        return this;
    }

    public FlurlRetryPolicyBuilder ShouldRetryNetworkErrors(bool value)
    {
        if (_shouldRetryNetworkErrors)
            _shouldRetryNetworkErrors = value;

        return this;
    }

    public FlurlRetryPolicyBuilder WithRetry(int retryCount, Func<int, TimeSpan> sleepDurationProvider)
    {
        _retryConfiguration = (retryCount, sleepDurationProvider);

        return this;
    }

    public async Task<T> Execute<T>(Func<Task<T>> makeRequest)
    {
        if (_retryConfiguration == null)
            throw new InvalidOperationException("Retry has not been configured.");

        var policyBuilder = Policy<T>.Handle<Exception>(_ => false);

        if (_statusCodes.Count == 0)
        {
            policyBuilder = policyBuilder.Or<FlurlHttpException>(exception =>
            {
                if (exception.StatusCode == null)
                    return false;

                return _statusCodes.Contains((int)exception.StatusCode);
            });
        }

        if (_shouldRetryClientTimeouts)
        {
            policyBuilder = policyBuilder.Or<FlurlHttpTimeoutException>(_ => true);
        }

        if (_shouldRetryNetworkErrors)
        {
            policyBuilder = policyBuilder.Or<SocketException>(_ => true);
        }

        var retryPolicy = policyBuilder.WaitAndRetryAsync(_retryConfiguration.Value.retryCount, _retryConfiguration.Value.sleepDurationProvider);
        var result = await retryPolicy.ExecuteAsync(makeRequest);

        return result;
    }
}
