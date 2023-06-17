using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models;

public class Content : BaseEntity, IConfigurable<Content>, ITrackable
{
    public int Id { get; set; }
    public int PlatformId { get; set; }
    public string IdOnPlatform { get; set; }
    public long ChangeId { get; set; }
    public string ViewLocation { get; set; }
    public bool IsSfw { get; set; }
    public int ThumbnailId { get; set; }
    public string Discriminator { get; set; }
    public bool IsDeleted { get; set; }

    public void Configure(EntityTypeBuilder<Content> entity)
    {
        entity.HasDiscriminator(e => e.Discriminator);

        entity.Property(e => e.Id)
            .ValueGeneratedNever();
        entity.HasKey(e => e.Id);

        entity.Property(e => e.IdOnPlatform)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.ChangeId);
        entity.HasIndex(e => new { e.PlatformId, e.ChangeId }).IsUnique();

        entity.Property(e => e.IsSfw);
        entity.HasIndex(e => e.IsSfw);

        entity.Property(e => e.ViewLocation).IsRequired();

        entity.Property(e => e.IsDeleted);

        entity.Property(e => e.PlatformId);

        entity.Property(e => e.ThumbnailId);
    }
}
