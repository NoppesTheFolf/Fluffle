using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Noppes.Fluffle.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigurationSectionAttribute : Attribute
    {
        public string Name { get; set; }

        public ConfigurationSectionAttribute(string name)
        {
            Name = name;
        }
    }

    public abstract class FluffleConfigurationPart<TConfiguration> : AbstractValidator<TConfiguration>
    {
    }

    public class FluffleConfiguration
    {
        /// <summary>
        /// Where the JSON configuration is located. By default in the application its root.
        /// </summary>
        private const string Location = "appsettings.json";

        private IConfigurationRoot Root { get; set; }

        private FluffleConfiguration()
        {
        }

        /// <summary>
        /// Load a <see cref="FluffleConfiguration"/> for the specified type. The type has influence
        /// on the user secrets configuration used.
        /// </summary>
        public static FluffleConfiguration Load<TFor>(bool useSerilog = true) where TFor : class =>
            Load(typeof(TFor), useSerilog);

        /// <summary>
        /// Load a <see cref="FluffleConfiguration"/> for the specified type. The type has influence
        /// on the user secrets configuration used.
        /// </summary>
        public static FluffleConfiguration Load(Type forType, bool useSerilog = true)
        {
            if (!forType.IsClass)
                throw new ArgumentException($"Type needs to be a class.", nameof(forType));

            var configuration = new FluffleConfiguration();

            if (useSerilog)
                Log.Logger = LoggerFactory.Create();

            // Having reloadOnChange set to false on the call to AddJsonFile is stupidly important.
            // For some unknown reason ASP.NET Core starts running out of available inotify
            // instances and eventually starts throwing errors.
            configuration.Root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Location, true, false)
                .AddUserSecrets(forType.GetTypeInfo().Assembly, true)
                .Build();

            return configuration;
        }

        /// <summary>
        /// Get the configuration instance of the specified type.
        /// </summary>
        public TConfiguration Get<TConfiguration>() => (TConfiguration)Get(typeof(TConfiguration));

        /// <summary>
        /// Get the configuration instance of the specified type.
        /// </summary>
        public object Get(Type configurationType)
        {
            // The name of the section which contains the configuration its values is defined by an
            // attribute placed on the configuration its class
            var sectionAttribute = configurationType.GetCustomAttribute<ConfigurationSectionAttribute>();

            if (sectionAttribute == null)
                throw new ArgumentException($"Configuration {configurationType.Name} is not decorated with the {nameof(ConfigurationSectionAttribute)} attribute.");

            var configuration = Root.GetSection(sectionAttribute.Name).Get(configurationType);

            if (configuration == null)
                throw new InvalidOperationException($"There exists no configuration matching '{sectionAttribute.Name}'.");

            // In order to prevent weird runtime errors, we apply validation if the configuration
            // supports it
            var validatorInterface = configurationType.GetInterface(typeof(IValidator<>).Name);
            if (validatorInterface == null)
                return configuration;

            var validationMethod = validatorInterface.GetMethod(nameof(IValidator<object>.Validate));

            if (validationMethod == null)
                throw new InvalidOperationException($"Couldn't find the validation method on type {configurationType.Name}.");

            var validationResult = (ValidationResult)validationMethod.Invoke(configuration, new[] { configuration });

            if (validationResult == null)
                throw new InvalidOperationException($"Calling the validation method on configuration {configurationType.Name} didn't produce a result.");

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            return configuration;
        }
    }
}
