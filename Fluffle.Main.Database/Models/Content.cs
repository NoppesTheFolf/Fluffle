using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models
{
    public interface ITrackable
    {
        public long? ChangeId { get; set; }
    }

    public partial class Content : TrackedBaseEntity, ITrackable, IConfigurable<Content>
    {
        public Content()
        {
            Files = new HashSet<ContentFile>();
            Credits = new HashSet<CreditableEntity>();
            ContentCreditableEntity = new HashSet<ContentCreditableEntity>();
            Warnings = new HashSet<ContentWarning>();
            Errors = new HashSet<ContentError>();
            Tags = new HashSet<Tag>();
            ContentTags = new HashSet<ContentTag>();
        }

        public int Id { get; set; }
        public int PlatformId { get; set; }
        public string IdOnPlatform { get; set; }

        /// <summary>
        /// Platforms commonly use auto increment integers to differentiate content. Having this ID
        /// available as integer can be very useful for syncing purposes.
        /// </summary>
        public int? IdOnPlatformAsInteger { get; set; }
        public string ViewLocation { get; set; }
        public int RatingId { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public long? ChangeId { get; set; }
        public int MediaTypeId { get; set; }
        public int LastEditedById { get; set; }
        public int? ThumbnailId { get; set; }
        public bool RequiresIndexing { get; set; }
        public bool IsIndexed { get; set; }
        public long ReservedUntil { get; set; }
        public string Discriminator { get; set; }
        public bool IsMarkedForDeletion { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ApiKey LastEditedBy { get; set; }
        public virtual ContentRating Rating { get; set; }
        public virtual Platform Platform { get; set; }
        public virtual MediaType MediaType { get; set; }
        public virtual Thumbnail Thumbnail { get; set; }
        public virtual ICollection<ContentFile> Files { get; set; }
        public virtual ICollection<CreditableEntity> Credits { get; set; }
        public virtual ICollection<ContentCreditableEntity> ContentCreditableEntity { get; set; }
        public virtual ICollection<ContentWarning> Warnings { get; set; }
        public virtual ICollection<ContentError> Errors { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<ContentTag> ContentTags { get; set; }

        public IEnumerable<Thumbnail> EnumerateThumbnails()
        {
            if (Thumbnail != null)
                yield return Thumbnail;
        }

        public void Configure(EntityTypeBuilder<Content> entity)
        {
            entity.HasDiscriminator(e => e.Discriminator);

            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IdOnPlatform)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(e => e.IdOnPlatform);

            entity.Property(e => e.PlatformId);
            entity.HasIndex(e => new { e.PlatformId, PlatformContentId = e.IdOnPlatform }).IsUnique();
            entity.HasOne(d => d.Platform)
                .WithMany(p => p.Content)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.IdOnPlatformAsInteger);
            entity.HasIndex(e => e.IdOnPlatformAsInteger);

            entity.Property(e => e.ChangeId);
            entity.HasIndex(e => e.ChangeId).IsUnique();

            // This speeds up image synchronization
            entity.HasIndex(e => new { e.Discriminator, e.ChangeId });

            entity.Property(e => e.ViewLocation)
                .IsRequired()
                .HasMaxLength(2048);

            entity.Property(e => e.IsIndexed);
            entity.Property(e => e.RequiresIndexing);

            entity.Property(e => e.ReservedUntil);

            entity.Property(e => e.Priority);
            entity.HasIndex(e => e.Priority);

            entity.Property(e => e.Title)
                .HasColumnType("character varying");

            entity.Property(e => e.IsMarkedForDeletion);
            entity.HasIndex(e => e.IsMarkedForDeletion);

            entity.Property(e => e.IsDeleted);
            entity.HasIndex(e => e.IsDeleted);

            // Index deleted content
            entity.HasIndex(e => new { e.IsDeleted, e.IsMarkedForDeletion });

            // Speeds up indexing statistics count
            entity.HasIndex(e => new { e.PlatformId, e.MediaTypeId, e.IsIndexed });

            entity.Property(e => e.MediaTypeId);
            entity.HasOne(e => e.MediaType)
                .WithMany(e => e.Content)
                .HasForeignKey(e => e.MediaTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.LastEditedById);
            entity.HasOne(d => d.LastEditedBy)
                .WithMany(p => p.LastEditedContent)
                .HasForeignKey(d => d.LastEditedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.RatingId);
            entity.HasOne(d => d.Rating)
                .WithMany(p => p.Content)
                .HasForeignKey(d => d.RatingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ThumbnailId);
            entity.HasOne(e => e.Thumbnail)
                .WithMany(e => e.Content)
                .HasForeignKey(e => e.ThumbnailId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Credits)
                .WithMany(e => e.Content)
                .UsingEntity<ContentCreditableEntity>(r =>
                {
                    return r.HasOne(e => e.CreditableEntity)
                        .WithMany(e => e.ContentCreditableEntity)
                        .HasForeignKey(e => e.CreditableEntityId);
                }, l =>
                {
                    return l.HasOne(e => e.Content)
                        .WithMany(e => e.ContentCreditableEntity)
                        .HasForeignKey(e => e.ContentId);
                });

            entity.HasMany(e => e.Tags)
                .WithMany(e => e.Content)
                .UsingEntity<ContentTag>(r =>
                {
                    return r.HasOne(e => e.Tag)
                        .WithMany(e => e.ContentTags)
                        .HasForeignKey(e => e.TagId);
                }, l =>
                {
                    return l.HasOne(e => e.Content)
                        .WithMany(e => e.ContentTags)
                        .HasForeignKey(e => e.ContentId);
                });
        }
    }

    public partial class Image : Content, IConfigurable<Image>
    {
        public bool HasTransparency { get; set; }

        public virtual ImageHash ImageHash { get; set; }

        public void Configure(EntityTypeBuilder<Image> entity)
        {
            entity.Property(e => e.HasTransparency);
        }
    }
}
