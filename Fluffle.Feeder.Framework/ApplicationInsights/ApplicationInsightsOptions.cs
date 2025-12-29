using System.ComponentModel.DataAnnotations;

namespace Fluffle.Feeder.Framework.ApplicationInsights;

internal class ApplicationInsightsOptions
{
    public const string ApplicationInsights = "ApplicationInsights";

    [Required]
    public required string ConnectionString { get; set; }
}
