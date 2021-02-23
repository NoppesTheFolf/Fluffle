using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Main.Api.Services;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api
{
    public class DeletionService : IService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DeletionService> _logger;

        public DeletionService(IServiceProvider services, ILogger<DeletionService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FluffleContext>();
                var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();

                var contentMarkedForDeletion = await context.Content
                    .Include(c => c.Platform)
                    .Where(c => c.IsMarkedForDeletion)
                    .Take(50)
                    .ToListAsync();

                if (!contentMarkedForDeletion.Any())
                    break;

                foreach (var content in contentMarkedForDeletion)
                {
                    _logger.LogInformation("Deleting content on platform {platformName} with ID {id}.",
                        content.Platform.Name, content.IdOnPlatform);

                    var error = await contentService.DeleteAsync(content.Platform.NormalizedName, content.IdOnPlatform);

                    if (error == null)
                        continue;

                    _logger.LogWarning("Deleting content on platform {platformName} with ID {id} failed with code {code}.",
                        content.Platform.Name, content.IdOnPlatform, error.Code);
                }
            }
        }
    }
}
