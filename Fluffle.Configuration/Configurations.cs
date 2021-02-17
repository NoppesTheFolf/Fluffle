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
    /// Contact information which is sent along with requests to certain APIs.
    /// </summary>
    [ConfigurationSection("Contact")]
    public class ContactConfiguration : FluffleConfigurationPart<ContactConfiguration>
    {
        /// <summary>
        /// The username on the specified <see cref="Platform"/>.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The platform on which you can be found.
        /// </summary>
        public string Platform { get; set; }

        public ContactConfiguration()
        {
            RuleFor(o => o.Username).NotEmpty();
            RuleFor(o => o.Platform).NotEmpty();
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
