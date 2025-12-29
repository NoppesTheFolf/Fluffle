using System.ComponentModel.DataAnnotations;

namespace Fluffle.Imaging.Api;

public class ImagingOptions
{
    public const string Imaging = "Imaging";

    [Required]
    public required int MaximumImageArea { get; set; }

    [Required]
    public required long MaximumFileSize { get; set; }
}
