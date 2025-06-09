namespace Fluffle.Imaging.Api.Models;

public class ImagingErrorModel
{
    public required ImagingErrorCode Code { get; set; }

    public required string Message { get; set; }
}
