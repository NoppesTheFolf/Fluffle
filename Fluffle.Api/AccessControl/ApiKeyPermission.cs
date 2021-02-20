using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Generic database entity representing the most basic way of attaching a API key to a specific permission.
    /// </summary>
    public abstract class ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission> : TrackedBaseEntity, IConfigurable<TApiKeyPermission>
        where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>
        where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>
        where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>
    {
        public int ApiKeyId { get; set; }

        public int PermissionId { get; set; }

        public virtual TApiKey ApiKey { get; set; }

        public virtual TPermission Permission { get; set; }

        public void Configure(EntityTypeBuilder<TApiKeyPermission> entity)
        {
            entity.Property(e => e.ApiKeyId);
            entity.Property(e => e.PermissionId);
            entity.HasKey(e => new { e.ApiKeyId, e.PermissionId });

            entity.HasOne(d => d.ApiKey)
                .WithMany(p => p.ApiKeyPermissions)
                .HasForeignKey(d => d.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Permission)
                .WithMany(p => p.ApiKeyPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
