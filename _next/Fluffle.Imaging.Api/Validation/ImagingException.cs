using Fluffle.Imaging.Api.Models;

namespace Fluffle.Imaging.Api.Validation;

public class ImagingException : Exception
{
    public ImagingErrorCode Code { get; }

    public ImagingException(ImagingErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
