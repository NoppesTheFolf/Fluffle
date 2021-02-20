﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Manages everything to do with permissions and API keys.
    /// </summary>
    public class AccessManager<TApiKey, TPermission, TApiKeyPermission>
        where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>, new()
        where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>, new()
        where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>, new()
    {
        private readonly ApiKeyContext<TApiKey, TPermission, TApiKeyPermission> _context;

        public AccessManager(ApiKeyContext<TApiKey, TPermission, TApiKeyPermission> context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates an API key using the given API key object. The key property will be generated by
        /// this method.
        /// </summary>
        public async Task CreateApiKeyAsync(TApiKey apiKey)
        {
            if (apiKey.Key == null)
            {
                do
                {
                    apiKey.Key = GenerateNewApiKey();
                } while (await _context.ApiKeys.AnyAsync(ak => ak.Key == apiKey.Key));
            }
            else
            {
                if (apiKey.Key.Length != 32)
                    throw new ArgumentException($"API keys need to be 32 characters long.", nameof(apiKey));
            }

            await _context.ApiKeys.AddAsync(apiKey);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generates a API with 32 characters and a 128 bit entropy using a <see cref="RNGCryptoServiceProvider"/>.
        /// </summary>
        private static string GenerateNewApiKey()
        {
            using var rng = new RNGCryptoServiceProvider();
            var buffer = new byte[16];
            rng.GetBytes(buffer);

            var keyBuilder = new StringBuilder();

            foreach (var part in buffer)
                keyBuilder.Append(part.ToString("x2"));

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Gets the API key with the provided key. Optionally also include the permissions.
        /// </summary>
        public Task<TApiKey> GetApiKeyAsync(string apiKey, bool includePermissions = false)
        {
            IQueryable<TApiKey> query = _context.ApiKeys;

            if (includePermissions)
            {
                query = query
                    .Include(ak => ak.ApiKeyPermissions)
                    .ThenInclude(akp => akp.Permission);
            }

            return query.FirstOrDefaultAsync(ak => ak.Key == apiKey);
        }

        /// <summary>
        /// Whether or not the API key with the given key exists.
        /// </summary>
        public Task<bool> ApiKeyExists(string key)
        {
            return _context.ApiKeys.AnyAsync(ap => ap.Key == key);
        }

        /// <summary>
        /// Grants the given API key the provided permission.
        /// </summary>
        public Task GrantPermission(TApiKey apiKey, TPermission permission)
        {
            return GrantPermission(apiKey.Key, permission.Name);
        }

        /// <summary>
        /// Grants the given API key the provided permission.
        /// </summary>
        public async Task GrantPermission(string key, string permissionName)
        {
            var apiKey = await GetApiKeyAsync(key);

            if (apiKey == null)
                throw new ArgumentException("API key does not exist.", nameof(key));

            var permission = await GetPermissionAsync(permissionName);

            if (permission == null)
                throw new ArgumentException("Permission with the given name does not exist.", nameof(permissionName));

            // Skip if the API key already has given permission
            if (await _context.ApiKeyPermissions.AnyAsync(akp => akp.ApiKeyId == apiKey.Id && akp.PermissionId == permission.Id))
                return;

            await _context.ApiKeyPermissions.AddAsync(new TApiKeyPermission
            {
                ApiKeyId = apiKey.Id,
                PermissionId = permission.Id
            });

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if the given permission is attached to any API key.
        /// </summary>
        public async Task<bool> IsPermissionInUseAsync(string permissionName)
        {
            var permission = await GetPermissionAsync(permissionName);

            if (permission == null)
                return false;

            return await _context.Set<TApiKeyPermission>()
                .AnyAsync(p => p.PermissionId == permission.Id);
        }

        /// <summary>
        /// Gets the permission with the given name.
        /// </summary>
        public Task<TPermission> GetPermissionAsync(string permissionName)
        {
            permissionName = NormalizeName(permissionName);

            return _context.Set<TPermission>().FirstOrDefaultAsync(p => p.Name == permissionName);
        }

        /// <summary>
        /// Gets the permissions contained in the database.
        /// </summary>
        public IQueryable<TPermission> GetPermissions()
        {
            return _context.Set<TPermission>();
        }

        /// <summary>
        /// Adds the given permission to the database.
        /// </summary>
        public async Task AddPermissionAsync(TPermission permission)
        {
            if (permission.Id != 0)
                throw new ArgumentException("Permission already exists.", nameof(permission));

            permission.Name = NormalizeName(permission.Name);

            if (await _context.Set<TPermission>().AnyAsync(p => p.Name == permission.Name))
                throw new ArgumentException("Permission with the given name already exists.", nameof(permission));

            await _context.Set<TPermission>().AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the permission with the given name. Will return false if the permission doesn't
        /// exist. True if it got deleted.
        /// </summary>
        public async Task<bool> RemovePermissionAsync(string permissionName)
        {
            var permission = await GetPermissionAsync(permissionName);

            if (permission == null)
                return false;

            _context.Set<TPermission>().Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Normalizes the given permission name.
        /// </summary>
        public string NormalizeName(string permissionName)
        {
            return permissionName.ToUpperInvariant();
        }
    }
}
