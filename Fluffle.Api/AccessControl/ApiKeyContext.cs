using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Generic database context which supports access control.
    /// </summary>
    public abstract class ApiKeyContext<TApiKey, TPermission, TApiKeyPermission> : BaseContext
        where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>
        where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>
        where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>
    {
        public virtual DbSet<TApiKey> ApiKeys { get; set; }

        public virtual DbSet<TPermission> Permissions { get; set; }

        public virtual DbSet<TApiKeyPermission> ApiKeyPermissions { get; set; }

        protected ApiKeyContext()
        {
        }

        protected ApiKeyContext(DbContextOptions options) : base(options)
        {
        }
    }
}
