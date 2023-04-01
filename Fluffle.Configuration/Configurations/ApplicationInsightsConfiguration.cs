using FluentValidation;

namespace Noppes.Fluffle.Configuration;

/// <summary>
/// Configuration used regarding Application Insights.
/// </summary>
[ConfigurationSection("ApplicationInsights")]
public class ApplicationInsightsConfiguration : FluffleConfigurationPart<ApplicationInsightsConfiguration>
{
    public string ConnectionString { get; set; }

    public ApplicationInsightsConfiguration()
    {
        RuleFor(o => o.ConnectionString).NotEmpty();
    }
}
