using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Database.Models;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api
{
    public class ApiInitializer
    {
        private static readonly ApiKey DevelopmentApiKey = new()
        {
            Key = "abaf209faeb68e360def14ac3df0d894",
            Description = "Development key"
        };

        private readonly IWebHostEnvironment _environment;
        private readonly FluffleContext _context;
        private readonly AccessManager<ApiKey, Permission, ApiKeyPermission> _accessManager;

        public ApiInitializer(IWebHostEnvironment environment, FluffleContext context,
            AccessManager<ApiKey, Permission, ApiKeyPermission> accessManager)
        {
            _environment = environment;
            _context = context;
            _accessManager = accessManager;
        }

        public async Task InitializeAsync()
        {
            _context.SyncWithDataSources();
            await _context.SaveChangesAsync();

            if (!_environment.IsDevelopment())
                return;

            var permissions = await _accessManager.GetPermissions().ToListAsync();

            if (!await _accessManager.ApiKeyExists(DevelopmentApiKey.Key))
                await _accessManager.CreateApiKeyAsync(DevelopmentApiKey);

            foreach (var permission in permissions)
                await _accessManager.GrantPermission(DevelopmentApiKey, permission);
        }
    }
}
