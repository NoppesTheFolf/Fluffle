namespace Fluffle.Vector.Api.Client;

public class VectorApiException : Exception
{
    public VectorApiException(string bodyContent, HttpRequestException innerException)
        : base(string.IsNullOrWhiteSpace(bodyContent) ? innerException.Message : bodyContent, innerException)
    {
    }
}
