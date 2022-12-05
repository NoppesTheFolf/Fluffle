using System;
using FluentValidation;
using System.Collections.Generic;

namespace Noppes.Fluffle.Configuration;

/// <summary>
/// Configuration regarding DeviantArt
/// </summary>
[ConfigurationSection("DeviantArt")]
public class DeviantArtConfiguration : FluffleConfigurationPart<DeviantArtConfiguration>
{
    public DeviantArtQueryDeviationsWatcherConfiguration QueryDeviationsWatcher { get; set; }

    public DeviantArtNewestDeviationsWatcherConfiguration NewestDeviationsWatcher { get; set; }

    public DeviantArtDeviationsProcessorConfiguration DeviationsProcessor { get; set; }

    public DeviantArtFurryArtistCheckerConfiguration FurryArtistChecker { get; set; }

    public DeviantArtGalleryScraperConfiguration GalleryScraper { get; set; }

    public DeviantArtCredentialsConfiguration Credentials { get; set; }

    public DeviantArtTagsConfiguration Tags { get; set; }

    public AzureStorageAccount StorageAccount { get; set; }

    public DeviantArtConfiguration()
    {
        RuleFor(x => x.QueryDeviationsWatcher).SetValidator(x => x.QueryDeviationsWatcher);
        RuleFor(x => x.NewestDeviationsWatcher).SetValidator(x => x.NewestDeviationsWatcher);
        RuleFor(x => x.DeviationsProcessor).SetValidator(x => x.DeviationsProcessor);
        RuleFor(x => x.FurryArtistChecker).SetValidator(x => x.FurryArtistChecker);
        RuleFor(x => x.GalleryScraper).SetValidator(x => x.GalleryScraper);

        RuleFor(x => x.Credentials).SetValidator(x => x.Credentials);
        RuleFor(x => x.Tags).SetValidator(x => x.Tags);
        RuleFor(x => x.StorageAccount).SetValidator(x => x.StorageAccount);
    }
}

public class DeviantArtGalleryScraperConfiguration : DeviantArtApplicationConfiguration<DeviantArtGalleryScraperConfiguration>
{
}

public class DeviantArtFurryArtistCheckerConfiguration : DeviantArtApplicationConfiguration<DeviantArtFurryArtistCheckerConfiguration>
{
    /// <summary>
    /// The number of deviations to get.
    /// </summary>
    public int N { get; set; }

    /// <summary>
    /// The number of deviations which need to contain a tag known to relate to the furry fandom.
    /// </summary>
    public int NFurry { get; set; }

    public DeviantArtFurryArtistCheckerConfiguration()
    {
        RuleFor(x => x.N).GreaterThanOrEqualTo(1);
        RuleFor(x => x.NFurry).GreaterThanOrEqualTo(0);
    }
}

public class DeviantArtApplicationConfiguration<T> : AbstractValidator<T> where T : DeviantArtApplicationConfiguration<T>
{
    public TimeSpan Interval { get; set; }

    public TimeSpan? QueueMessagesVisibleAfter { get; set; }

    public TimeSpan? ClientThrottle { get; set; }

    public DeviantArtApplicationConfiguration()
    {
        RuleFor(x => x.Interval).NotEmpty().GreaterThanOrEqualTo(TimeSpan.Zero);
        RuleFor(x => x.QueueMessagesVisibleAfter).GreaterThanOrEqualTo(TimeSpan.Zero);
        RuleFor(x => x.ClientThrottle).GreaterThanOrEqualTo(TimeSpan.Zero);
    }
}

public class DeviantArtDeviationsProcessorConfiguration : DeviantArtApplicationConfiguration<DeviantArtDeviationsProcessorConfiguration>
{
    public int AtLeastActiveFor { get; set; }
}

public class DeviantArtNewestDeviationsWatcherConfiguration : DeviantArtApplicationConfiguration<DeviantArtNewestDeviationsWatcherConfiguration>
{
}

public class DeviantArtQueryDeviationsWatcherConfiguration : DeviantArtApplicationConfiguration<DeviantArtQueryDeviationsWatcherConfiguration>
{
}

public class AzureStorageAccount : AbstractValidator<AzureStorageAccount>
{
    public string ConnectionString { get; set; }

    public AzureStorageAccount()
    {
        RuleFor(x => x.ConnectionString).NotEmpty();
    }
}

public class DeviantArtTagsConfiguration : AbstractValidator<DeviantArtTagsConfiguration>
{
    public ICollection<string> Furry { get; set; }

    public ICollection<string> General { get; set; }

    public DeviantArtTagsConfiguration()
    {
        RuleFor(x => x.Furry).NotEmpty();
        RuleFor(x => x.General).NotEmpty();
    }
}

public class DeviantArtCredentialsConfiguration : AbstractValidator<DeviantArtCredentialsConfiguration>
{
    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public DeviantArtCredentialsConfiguration()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientSecret).NotEmpty();
    }
}

[ConfigurationSection("DeviantArtDatabase")]
public class DeviantArtDatabaseConfiguration : DatabaseConfiguration
{
}
