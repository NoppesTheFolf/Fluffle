using System.Net;

namespace Noppes.Fluffle.Api.Services;

/// <summary>
/// Represents an error produced by a <see cref="Service"/>. SE is an abbreviation for
/// ServiceError to improve readability due to common usage.
/// </summary>
public class SE
{
    public string Code { get; }

    public string Message { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public SE(string code, HttpStatusCode httpStatusCode, string message)
    {
        Code = code;
        HttpStatusCode = httpStatusCode;
        Message = message;
    }
}
