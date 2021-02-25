﻿namespace Noppes.Fluffle.Api
{
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
}