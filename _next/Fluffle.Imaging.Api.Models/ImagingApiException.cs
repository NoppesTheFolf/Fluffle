namespace Fluffle.Imaging.Api.Models;

public class ImagingApiException : Exception
{
    public ImagingErrorCode Code { get; }

    public ImagingApiException(ImagingErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
