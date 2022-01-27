using FluentValidation;
using Noppes.Fluffle.Validation;
using System.Collections.Generic;

namespace Noppes.Fluffle.Configuration
{
    /// <summary>
    /// A very basic configuration class for databases. Provides a connection string for Entity
    /// Framework Core.
    /// </summary>
    public abstract class DatabaseConfiguration : FluffleConfigurationPart<DatabaseConfiguration>
    {
        /// <summary>
        /// Where may thou access thy database?
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// What is thy database bid?
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// What is thy name?
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// What's the special code word?
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// A connection string for easy use with EF Core.
        /// </summary>
        public string ConnectionString => $"Host={Host};Database={Database};Username={Username};Password={Password}";

        /// <summary>
        /// Number of seconds before a query times out.
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Whether or not to log queries to the console.
        /// </summary>
        public bool EnableLogging { get; set; }

        protected DatabaseConfiguration()
        {
            RuleFor(o => o.Host).Hostname();
            RuleFor(o => o.Database).NotEmpty();
            RuleFor(o => o.Username).NotEmpty();
            RuleFor(o => o.Password).NotEmpty();
            RuleFor(o => o.CommandTimeout).GreaterThan(0);
        }
    }

    /// <summary>
    /// The database configuration for the main server.
    /// </summary>
    [ConfigurationSection("MainDatabase")]
    public class MainDatabaseConfiguration : DatabaseConfiguration
    {
    }

    [ConfigurationSection("MainServer")]
    public class MainServerConfiguration : FluffleConfigurationPart<MainServerConfiguration>
    {
        /// <summary>
        /// Interval in minutes between updating indexing statistics.
        /// </summary>
        public int IndexingStatisticsInterval { get; set; }

        /// <summary>
        /// Interval in minutes between deleting content marked for deletion.
        /// </summary>
        public int DeletionInterval { get; set; }

        /// <summary>
        /// Interval in minutes between updating priorities for creditable entities.
        /// </summary>
        public int CreditableEntityPriorityInterval { get; set; }

        /// <summary>
        /// Time span in minutes between (re)calculating a creditable entity its priority.
        /// </summary>
        public int CreditableEntityPriorityExpirationTime { get; set; }

        public MainServerConfiguration()
        {
            RuleFor(o => o.IndexingStatisticsInterval).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(o => o.DeletionInterval).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(o => o.CreditableEntityPriorityInterval).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(o => o.CreditableEntityPriorityExpirationTime).NotEmpty().GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// The database configuration for search server instances.
    /// </summary>
    [ConfigurationSection("SearchDatabase")]
    public class SearchDatabaseConfiguration : DatabaseConfiguration
    {
    }

    /// <summary>
    /// The database configuration for the Twitter synchronization client.
    /// </summary>
    [ConfigurationSection("TwitterDatabase")]
    public class TwitterDatabaseConfiguration : DatabaseConfiguration
    {
    }

    /// <summary>
    /// Configuration regarding the compare API.
    /// </summary>
    [ConfigurationSection("Compare")]
    public class CompareConfiguration : FluffleConfigurationPart<CompareConfiguration>
    {
        /// <summary>
        /// Where the compare API is being hosted.
        /// </summary>
        public string Url { get; set; }

        public CompareConfiguration()
        {
            RuleFor(o => o.Url).NotEmpty();
        }
    }

    /// <summary>
    /// Configuration used regarding thumbnails, in specific when uploading them somewhere for
    /// permanent storage.
    /// </summary>
    [ConfigurationSection("Thumbnail")]
    public class ThumbnailConfiguration : FluffleConfigurationPart<ThumbnailConfiguration>
    {
        /// <summary>
        /// A salt which is used to randomize thumbnail name generation. Using a secret salt,
        /// prevents Fluffle from being used as some kind of thumbnail CDN for furry content, which
        /// is obviously not the goal of the service.
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// This is the base URL used after uploading the thumbnail to whichever domain.
        /// </summary>
        public string BaseUrl { get; set; }

        public ThumbnailConfiguration()
        {
            RuleFor(o => o.Salt).NotNull().Length(32);
            RuleFor(o => o.BaseUrl).NotNull();
        }
    }

    /// <summary>
    /// Backblaze B2 API configuration.
    /// </summary>
    [ConfigurationSection("BackblazeB2")]
    public class BackblazeB2Configuration : FluffleConfigurationPart<BackblazeB2Configuration>
    {
        /// <summary>
        /// The unique identifier of the <see cref="ApplicationKey"/>? No idea. It's required tho!
        /// </summary>
        public string ApplicationKeyId { get; set; }

        /// <summary>
        /// Uhhh, another part which makes up the entire API key? Cool.
        /// </summary>
        public string ApplicationKey { get; set; }

        public BackblazeB2Configuration()
        {
            RuleFor(o => o.ApplicationKeyId).NotEmpty();
            RuleFor(o => o.ApplicationKey).NotEmpty();
        }
    }

    /// <summary>
    /// Configuration regarding blacklisted tags.
    /// </summary>
    [ConfigurationSection("Blacklist")]
    public class BlacklistConfiguration : FluffleConfigurationPart<BlacklistConfiguration>
    {
        /// <summary>
        /// A universally applied blacklist of tags.
        /// </summary>
        public ICollection<string> Universal { get; set; }

        /// <summary>
        /// Blacklisted tags which are only applied to NSFW content.
        /// </summary>
        public ICollection<string> Nsfw { get; set; }

        public BlacklistConfiguration()
        {
            RuleFor(o => o.Universal).NotNull();
            RuleFor(o => o.Nsfw).NotNull();
        }
    }

    /// <summary>
    /// Configuration regarding e621.net.
    /// </summary>
    [ConfigurationSection("E621")]
    public class E621Configuration : FluffleConfigurationPart<E621Configuration>
    {
        /// <summary>
        /// Part of the login credentials. The username to authenticate with.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Part of the login credentials. The API key to authenticate with.
        /// </summary>
        public string ApiKey { get; set; }

        public E621Configuration()
        {
            RuleFor(o => o.Username).NotEmpty();
            RuleFor(o => o.ApiKey).Length(24);
        }
    }

    /// <summary>
    /// Configuration regarding furrynetwork.com.
    /// </summary>
    [ConfigurationSection("FurryNetwork")]
    public class FurryNetworkConfiguration : FluffleConfigurationPart<FurryNetworkConfiguration>
    {
        /// <summary>
        /// Refresh token which can be used to get an OAuth bearer token with.
        /// </summary>
        public string Token { get; set; }

        public FurryNetworkConfiguration()
        {
            RuleFor(o => o.Token).NotNull().Length(40);
        }
    }

    /// <summary>
    /// Configuration regarding furaffinity.net.
    /// </summary>
    [ConfigurationSection("FurAffinity")]
    public class FurAffinityConfiguration : FluffleConfigurationPart<FurAffinityConfiguration>
    {
        /// <summary>
        /// Confusing authentication token named 'A'.
        /// </summary>
        public string A { get; set; }

        /// <summary>
        /// Confusing authentication token named 'B'.
        /// </summary>
        public string B { get; set; }

        public FurAffinityConfiguration()
        {
            RuleFor(o => o.A).NotEmpty().Length(36);
            RuleFor(o => o.B).NotEmpty().Length(36);
        }
    }

    /// <summary>
    /// Configuration regarding weasyl.com.
    /// </summary>
    [ConfigurationSection("Weasyl")]
    public class WeasylConfiguration : FluffleConfigurationPart<WeasylConfiguration>
    {
        /// <summary>
        /// API authentication key.
        /// </summary>
        public string ApiKey { get; set; }

        public WeasylConfiguration()
        {
            RuleFor(o => o.ApiKey).NotEmpty().Length(48);
        }
    }

    /// <summary>
    /// Configuration regarding twitter.com.
    /// </summary>
    [ConfigurationSection("Twitter")]
    public class TwitterConfiguration : FluffleConfigurationPart<TwitterConfiguration>
    {
        public string ApiKey { get; set; }

        public string ApiKeySecret { get; set; }

        public string BearerToken { get; set; }

        public TwitterConfiguration()
        {
            RuleFor(o => o.ApiKey).NotEmpty();
            RuleFor(o => o.ApiKeySecret).NotEmpty();
            RuleFor(o => o.BearerToken).NotEmpty();
        }
    }

    [ConfigurationSection("TwitterSync")]
    public class TwitterSyncConfiguration : FluffleConfigurationPart<TwitterSyncConfiguration>
    {
        public int DownloadInterval { get; set; } = 200;

        public TwitterSyncConfiguration()
        {
            RuleFor(o => o.DownloadInterval).GreaterThanOrEqualTo(0);
        }
    }

    public abstract class SyncClientConfiguration<T> : FluffleConfigurationPart<T> where T : SyncClientConfiguration<T>
    {
        public int Interval { get; set; }

        protected SyncClientConfiguration()
        {
            RuleFor(o => o.Interval).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Configuration regarding Fur Affinity sync client.
    /// </summary>
    [ConfigurationSection("FurAffinitySync")]
    public class FurAffinitySyncConfiguration : SyncClientConfiguration<FurAffinitySyncConfiguration>
    {
        public int RecentSubmissionsInterval { get; set; }

        public int RecentSubmissionPriorityThreshold { get; set; }

        public int BelowBotLimitInterval { get; set; }

        public int AboveBotLimitInterval { get; set; }

        public FurAffinitySyncConfiguration()
        {
            RuleFor(o => o.RecentSubmissionsInterval).GreaterThanOrEqualTo(0);
            RuleFor(o => o.RecentSubmissionPriorityThreshold).GreaterThanOrEqualTo(0);
            RuleFor(o => o.BelowBotLimitInterval).GreaterThanOrEqualTo(0);
            RuleFor(o => o.AboveBotLimitInterval).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Configuration regarding indexing clients.
    /// </summary>
    [ConfigurationSection("Index")]
    public class IndexConfiguration : FluffleConfigurationPart<IndexConfiguration>
    {
        public class ProducerConfiguration : AbstractValidator<ProducerConfiguration>
        {
            public int Threads { get; set; }

            public ProducerConfiguration()
            {
                RuleFor(o => o.Threads).GreaterThanOrEqualTo(1);
            }
        }

        public class ConsumerConfiguration : AbstractValidator<ConsumerConfiguration>
        {
            public int Threads { get; set; }

            public int Buffer { get; set; }

            public ConsumerConfiguration()
            {
                RuleFor(o => o.Threads).GreaterThanOrEqualTo(1);
                RuleFor(o => o.Buffer).GreaterThanOrEqualTo(1);
            }
        }

        public class ClientConfiguration : AbstractValidator<ClientConfiguration>
        {
            public int Threads { get; set; }

            public int Interval { get; set; }

            public ClientConfiguration()
            {
                RuleFor(o => o.Threads).GreaterThanOrEqualTo(1);
                RuleFor(o => o.Interval).GreaterThanOrEqualTo(0);
            }
        }

        public ConsumerConfiguration ImageHasher { get; set; }

        public ConsumerConfiguration Thumbnailer { get; set; }

        public ConsumerConfiguration ThumbnailPublisher { get; set; }

        public ProducerConfiguration IndexPublisher { get; set; }

        public ClientConfiguration E621 { get; set; }

        public ClientConfiguration FurryNetwork { get; set; }

        public ClientConfiguration FurAffinity { get; set; }

        public ClientConfiguration Weasyl { get; set; }

        public ClientConfiguration Twitter { get; set; }

        public IndexConfiguration()
        {
            RuleFor(o => o.ImageHasher).NotEmpty().SetValidator(o => o.ImageHasher);
            RuleFor(o => o.Thumbnailer).NotEmpty().SetValidator(o => o.Thumbnailer);
            RuleFor(o => o.ThumbnailPublisher).NotEmpty().SetValidator(o => o.ThumbnailPublisher);
            RuleFor(o => o.IndexPublisher).NotEmpty().SetValidator(o => o.IndexPublisher);

            RuleFor(o => o.E621).NotEmpty().SetValidator(o => o.E621);
            RuleFor(o => o.FurryNetwork).NotEmpty().SetValidator(o => o.FurryNetwork);
            RuleFor(o => o.FurAffinity).NotEmpty().SetValidator(o => o.FurAffinity);
            RuleFor(o => o.Weasyl).NotEmpty().SetValidator(o => o.Weasyl);
            RuleFor(o => o.Twitter).NotEmpty().SetValidator(o => o.Twitter);
        }
    }

    /// <summary>
    /// Configuration regarding the main server as a client.
    /// </summary>
    [ConfigurationSection("Main")]
    public class MainConfiguration : FluffleConfigurationPart<MainConfiguration>
    {
        /// <summary>
        /// Where the main server is being hosted.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// API key which grants access to certain endpoints on the main server.
        /// </summary>
        public string ApiKey { get; set; }

        public MainConfiguration()
        {
            RuleFor(o => o.Url).NotEmpty();
            RuleFor(o => o.ApiKey).NotEmpty();
        }
    }

    /// <summary>
    /// Configuration regarding the prediction API.
    /// </summary>
    [ConfigurationSection("Prediction")]
    public class PredictionConfiguration : FluffleConfigurationPart<PredictionConfiguration>
    {
        /// <summary>
        /// Where the prediction API is running.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// API key required to access the prediction API.
        /// </summary>
        public string ApiKey { get; set; }

        public int ClassifyDegreeOfParallelism { get; set; } = 1;

        public PredictionConfiguration()
        {
            RuleFor(o => o.Url).NotEmpty();
            RuleFor(o => o.ApiKey).NotEmpty();
            RuleFor(o => o.ClassifyDegreeOfParallelism).GreaterThanOrEqualTo(1);
        }
    }
}
