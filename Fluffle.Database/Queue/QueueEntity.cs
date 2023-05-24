using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Noppes.Fluffle.Database.Queue;

public abstract class QueueEntity : BaseEntity
{
    public long Id { get; set; }

    public byte[] Data { get; set; }

    public long Priority { get; set; }

    protected void Configure<T>(EntityTypeBuilder<T> entity) where T : QueueEntity
    {
        entity.Property(x => x.Id);
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Data);

        entity.Property(x => x.Priority);
        entity.HasIndex(x => x.Priority);
    }
}
