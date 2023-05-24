using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.AccessControl;

/// <summary>
/// A seeder which can be ran at application startup to make sure the permissions defined in the
/// application also exist in the database.
/// </summary>
public abstract class PermissionSeeder : IService
{
    public abstract Task RunAsync();
}

/// <summary>
/// Generic implementation of <see cref="PermissionSeeder"/>. A seeder which can be ran at
/// application startup to make sure the permissions defined in the application also exist in
/// the database.
/// </summary>
public class PermissionSeeder<TApiKey, TPermission, TApiKeyPermission> : PermissionSeeder
    where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>, new()
    where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>, new()
    where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>, new()
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PermissionSeeder> _logger;

    public PermissionSeeder(IServiceProvider services, ILogger<PermissionSeeder> logger)
    {
        _services = services;
        _logger = logger;
    }

    public override async Task RunAsync()
    {
        _logger.LogInformation("Synchronizing permissions...");

        using var scope = _services.CreateScope();
        var permissionManager = scope.ServiceProvider
            .GetRequiredService<AccessManager<TApiKey, TPermission, TApiKeyPermission>>();

        // First get all the classes which implement the Permissions class. Then get all the
        // public static fields in said class marked with the permission attribute, this will
        // include constants too. Get those fields their value and normalize said values using
        // the permission manager.
        var permissions = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t != typeof(Permissions))
            .Where(t => typeof(Permissions).IsAssignableFrom(t))
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.GetCustomAttribute<PermissionAttribute>() != null)
            .Select(f => (string)f.GetValue(null))
            .Select(p => permissionManager.NormalizeName(p))
            .ToList();

        // Get all of the (normalized) permission names from the database
        var dbPermissions = permissionManager.GetPermissions()
            .Select(p => p.Name)
            .ToHashSet();

        // Create the permissions which are not in the database
        var missingPermissions = permissions
            .Where(p => !dbPermissions.Contains(p));

        foreach (var missingPermission in missingPermissions)
        {
            if (dbPermissions.Contains(missingPermission))
                continue;

            _logger.LogInformation("Adding permission {permissionName}.", missingPermission);
            await permissionManager.AddPermissionAsync(new TPermission
            {
                Name = missingPermission
            });
        }

        // Check which permissions are defined in the database, but not in the application.
        // These are considered redundant and are removed if they're not in use.
        dbPermissions.ExceptWith(permissions);

        foreach (var redundantPermission in dbPermissions)
        {
            if (await permissionManager.IsPermissionInUseAsync(redundantPermission))
            {
                _logger.LogWarning("Redundant permission {redundantPermission} is still used and therefore won't be removed.", redundantPermission);
                continue;
            }

            _logger.LogInformation("Removing redundant permission {redundantPermission}.", redundantPermission);
            await permissionManager.RemovePermissionAsync(redundantPermission);
        }

        _logger.LogInformation("Finished synchronizing permissions.");
    }
}
