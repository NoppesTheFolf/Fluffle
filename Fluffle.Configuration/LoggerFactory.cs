using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace Noppes.Fluffle.Configuration;

/// <summary>
/// Provides a method to create new Serilog logger instance.
/// </summary>
public static class LoggerFactory
{
    /// <summary>
    /// Creates a new Serilog logger which writes to the console. It uses the information log
    /// level if there is no debugger attached. The debug log level will be used with a debugger attached.
    /// </summary>
    public static ILogger Create(Action<LoggerConfiguration> configureLogging = null)
    {
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // Don't log Entity Framework Core queries and such
            .WriteTo.Console();

        configuration.MinimumLevel.Is(Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information);

        configureLogging?.Invoke(configuration);

        return configuration.CreateLogger();
    }
}
