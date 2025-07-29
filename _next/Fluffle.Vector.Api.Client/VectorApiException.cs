using System.Net;

namespace Fluffle.Vector.Api.Client;

public class VectorApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public VectorApiException(HttpStatusCode? statusCode, string bodyContent, HttpRequestException innerException)
        : base(string.IsNullOrWhiteSpace(bodyContent) ? innerException.Message : bodyContent, innerException)
    {
        StatusCode = statusCode;
    }
}
