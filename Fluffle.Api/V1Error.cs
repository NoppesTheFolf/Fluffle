using System.Collections.Generic;

namespace Noppes.Fluffle.Api;

/// <summary>
/// The error response returned by version 1 of the API.
/// </summary>
public class V1Error
{
    public string Code { get; set; }

    public string Message { get; set; }

    public V1Error()
    {
    }

    public V1Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
}

public class V1ValidationError : V1Error
{
    public IDictionary<string, IEnumerable<string>> Errors { get; set; }

    public V1ValidationError()
    {
    }

    public V1ValidationError(IDictionary<string, IEnumerable<string>> errors)
    {
        Errors = errors;
    }

    public V1ValidationError(string code, string message, IDictionary<string, IEnumerable<string>> errors) : base(code, message)
    {
        Errors = errors;
    }
}

/// <summary>
/// The error response returned by version 1 of the API including a trace ID for when unexpected
/// errors happen.
/// </summary>
public class TracedV1Error : V1Error
{
    public string TraceId { get; set; }

    public TracedV1Error(string code, string traceId, string message) : base(code, message)
    {
        TraceId = traceId;
    }

    public TracedV1Error()
    {
    }
}
