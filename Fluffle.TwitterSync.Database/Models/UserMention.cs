using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class UserMention : BaseEntity, IConfigurable<UserMention>
    {
        public string TweetId { get; set; }
        public virtual Tweet Tweet { get; set; }

        public string UserId { get; set; }

        public void Configure(EntityTypeBuilder<UserMention> entity)
        {
            entity.Property(e => e.TweetId).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Tweet)
                .WithMany(e => e.Mentions)
                .HasForeignKey(e => e.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.UserId).HasMaxLength(20).IsRequired();
            entity.HasKey(e => new { e.TweetId, e.UserId });
        }
    }
}
