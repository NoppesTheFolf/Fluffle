﻿using FluentValidation;
using Noppes.Fluffle.Validation;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Configuration;

public class MongoDbConfiguration : FluffleConfigurationPart<MongoDbConfiguration>
{
    public string ConnectionString { get; set; }

    public string Database { get; set; }

    public MongoDbConfiguration()
    {
        RuleFor(o => o.ConnectionString).NotEmpty();
        RuleFor(o => o.Database).NotEmpty();
    }
}

/// <summary>
/// A very basic configuration class for databases. Provides a connection string for Entity
/// Framework Core.
/// </summary>
public abstract class DatabaseConfiguration : FluffleConfigurationPart<DatabaseConfiguration>
{
    /// <summary>
    /// Where the database server is hosted.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Port at which the database server is hosted.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Name of the database itself.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Username of the user with which to connect to the database server.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Password of the user with which to connect to the database server.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// A connection string for easy use with EF Core.
    /// </summary>
    public string ConnectionString => $"Host={Host};Port={Port ?? 5432};Database={Database};Username={Username};Password={Password}";

    /// <summary>
    /// Number of seconds before a query times out.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    protected DatabaseConfiguration()
    {
        RuleFor(o => o.Host).Hostname();
        RuleFor(o => o.Port).GreaterThan(0);
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

[ConfigurationSection("SearchServer")]
public class SearchServerConfiguration : FluffleConfigurationPart<SearchServerConfiguration>
{
    public string SearchResultsTemporaryLocation { get; set; }

    public BackblazeB2Configuration SearchResultsBackblazeB2 { get; set; }

    public string SimilarityDataDumpLocation { get; set; }

    public TimeSpan SimilarityDataDumpInterval { get; set; }

    public SearchServerConfiguration()
    {
        RuleFor(o => o.SearchResultsTemporaryLocation).NotEmpty();
        RuleFor(o => o.SearchResultsBackblazeB2).NotEmpty().SetValidator(o => o.SearchResultsBackblazeB2);

        RuleFor(o => o.SimilarityDataDumpLocation).NotEmpty();
        RuleFor(o => o.SimilarityDataDumpInterval).GreaterThan(TimeSpan.Zero);
    }
}

[ConfigurationSection("Bot")]
public class BotConfiguration : FluffleConfigurationPart<BotConfiguration>
{
    public string TelegramToken { get; set; }

    public string TelegramHost { get; set; }

    public int TelegramGlobalBurstLimit { get; set; }

    public int TelegramGlobalBurstInterval { get; set; }

    public int TelegramGroupBurstLimit { get; set; }

    public int TelegramGroupBurstInterval { get; set; }

    public ICollection<string> TelegramKnownSources { get; set; }

    public string MongoConnectionString { get; set; }

    public string MongoDatabase { get; set; }

    public class ReverseSearchConfiguration : AbstractValidator<ReverseSearchConfiguration>
    {
        public int Workers { get; set; }

        public class RateLimiterConfiguration : AbstractValidator<RateLimiterConfiguration>
        {
            public int Count { get; set; }

            public int ExpirationTime { get; set; }

            public int PressureTimeSpan { get; set; }

            public int SaveEveryNthIncrement { get; set; }

            public RateLimiterConfiguration()
            {
                RuleFor(o => o.Count).GreaterThan(0);
                RuleFor(o => o.ExpirationTime).GreaterThan(0);
                RuleFor(o => o.PressureTimeSpan).GreaterThan(0);
                RuleFor(o => o.SaveEveryNthIncrement).GreaterThan(0);
            }
        }

        public RateLimiterConfiguration RateLimiter { get; set; }

        public ReverseSearchConfiguration()
        {
            RuleFor(o => o.Workers).GreaterThan(0);
            RuleFor(o => o.RateLimiter).NotEmpty().SetValidator(o => o.RateLimiter);
        }
    }

    public ReverseSearchConfiguration ReverseSearch { get; set; }

    public class CleanerConfiguration : AbstractValidator<CleanerConfiguration>
    {
        public int Interval { get; set; }

        public int ExpirationTime { get; set; }

        public CleanerConfiguration()
        {
            RuleFor(o => o.Interval).GreaterThan(0);
            RuleFor(o => o.ExpirationTime).GreaterThan(0);
        }
    }

    public CleanerConfiguration MessageCleaner { get; set; }

    public class BotBackblazeB2Configuration : BackblazeB2Configuration<BotBackblazeB2Configuration>
    {
        public int Workers { get; set; }

        public BotBackblazeB2Configuration()
        {
            RuleFor(o => o.Workers).NotEmpty().GreaterThan(0);
        }
    }

    public BotBackblazeB2Configuration IndexBackblazeB2 { get; set; }

    public BotBackblazeB2Configuration ThumbnailBackblazeB2 { get; set; }

    public string FluffleBaseUrl { get; set; }

    public BotConfiguration()
    {
        RuleFor(o => o.TelegramToken).NotEmpty();
        RuleFor(o => o.TelegramHost).NotEmpty();

        RuleFor(o => o.TelegramGlobalBurstLimit).GreaterThan(0);
        RuleFor(o => o.TelegramGlobalBurstInterval).GreaterThan(0);
        RuleFor(o => o.TelegramGroupBurstLimit).GreaterThan(0);
        RuleFor(o => o.TelegramGroupBurstInterval).GreaterThan(0);

        RuleFor(o => o.TelegramKnownSources).NotEmpty();

        RuleFor(o => o.MongoConnectionString).NotEmpty();
        RuleFor(o => o.MongoDatabase).NotEmpty();

        RuleFor(o => o.ReverseSearch).NotEmpty().SetValidator(o => o.ReverseSearch);

        RuleFor(o => o.MessageCleaner).NotEmpty().SetValidator(o => o.MessageCleaner);

        RuleFor(o => o.IndexBackblazeB2).NotEmpty().SetValidator(o => o.IndexBackblazeB2);
        RuleFor(o => o.ThumbnailBackblazeB2).NotEmpty().SetValidator(o => o.ThumbnailBackblazeB2);

        RuleFor(o => o.FluffleBaseUrl).NotEmpty().IsWellFormedHttpUrl();
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
public class BackblazeB2Configuration<T> : FluffleConfigurationPart<T> where T : BackblazeB2Configuration<T>
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

public class BackblazeB2Configuration : BackblazeB2Configuration<BackblazeB2Configuration>
{
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

[ConfigurationSection("Queuey")]
public class QueueyConfiguration : FluffleConfigurationPart<QueueyConfiguration>
{
    public string Url { get; set; }

    public string ApiKey { get; set; }

    public QueueyConfiguration()
    {
        RuleFor(o => o.Url).NotEmpty();
        RuleFor(o => o.ApiKey).NotEmpty();
    }
}

[ConfigurationSection("MlApi")]
public class MlApiConfiguration : FluffleConfigurationPart<MlApiConfiguration>
{
    public string Url { get; set; }

    public string ApiKey { get; set; }

    public MlApiConfiguration()
    {
        RuleFor(o => o.Url).NotEmpty();
        RuleFor(o => o.ApiKey).NotEmpty();
    }
}

[ConfigurationSection("TwitterApi")]
public class TwitterApiConfiguration : FluffleConfigurationPart<TwitterApiConfiguration>
{
    public string Url { get; set; }

    public string ApiKey { get; set; }

    public TwitterApiConfiguration()
    {
        RuleFor(o => o.Url).NotEmpty();
        RuleFor(o => o.ApiKey).NotEmpty();
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

    public MongoDbConfiguration MongoDb { get; set; }

    public TwitterSyncConfiguration()
    {
        RuleFor(o => o.DownloadInterval).GreaterThanOrEqualTo(0);
        RuleFor(o => o.MongoDb).NotEmpty().SetValidator(o => o.MongoDb);
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
    public int BelowBotLimitInterval { get; set; }

    public int AboveBotLimitInterval { get; set; }

    public FurAffinitySyncConfiguration()
    {
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

    public ClientConfiguration DeviantArt { get; set; }

    public ClientConfiguration Inkbunny { get; set; }

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
        RuleFor(o => o.DeviantArt).NotEmpty().SetValidator(o => o.DeviantArt);
        RuleFor(o => o.Inkbunny).NotEmpty().SetValidator(o => o.Inkbunny);
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
