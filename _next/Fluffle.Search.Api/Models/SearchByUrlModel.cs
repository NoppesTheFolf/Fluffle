using System.ComponentModel.DataAnnotations;

namespace Fluffle.Search.Api.Models;

public class SearchByUrlModel
{
    [Required]
    public required string Url { get; set; }

    [Required]
    public required int Limit { get; set; }
}
