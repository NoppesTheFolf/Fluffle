﻿using FluentValidation;
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
        protected DatabaseConfiguration()
        {
            RuleFor(o => o.Host).Hostname();
            RuleFor(o => o.Database).NotEmpty();
            RuleFor(o => o.Username).NotEmpty();
            RuleFor(o => o.Password).NotEmpty();
        }

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
    }

    /// <summary>
    /// The database configuration for the main server.
    /// </summary>
    [ConfigurationSection("MainDatabase")]
    public class MainDatabaseConfiguration : DatabaseConfiguration
    {
    }

    /// <summary>
    /// The database configuration for search server instances.
    /// </summary>
    [ConfigurationSection("SearchDatabase")]
    public class SearchDatabaseConfiguration : DatabaseConfiguration
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
    /// Configuration regarding indexing client.
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

        public IndexConfiguration()
        {
            RuleFor(o => o.ImageHasher).NotEmpty().SetValidator(o => o.ImageHasher);
            RuleFor(o => o.Thumbnailer).NotEmpty().SetValidator(o => o.Thumbnailer);
            RuleFor(o => o.ThumbnailPublisher).NotEmpty().SetValidator(o => o.ThumbnailPublisher);
            RuleFor(o => o.IndexPublisher).NotEmpty().SetValidator(o => o.IndexPublisher);

            RuleFor(o => o.E621).NotEmpty().SetValidator(o => o.E621);
            RuleFor(o => o.FurryNetwork).NotEmpty().SetValidator(o => o.FurryNetwork);
            RuleFor(o => o.FurAffinity).NotEmpty().SetValidator(o => o.FurAffinity);
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
}
