﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Database.Models
{
    public partial class CreditableEntity : BaseEntity, ITrackable, IConfigurable<CreditableEntity>
    {
        public CreditableEntity()
        {
            Content = new HashSet<Content>();
            ContentCreditableEntity = new HashSet<ContentCreditableEntity>();
        }

        public int Id { get; set; }

        public string IdOnPlatform { get; set; }

        public int PlatformId { get; set; }

        public string Name { get; set; }

        public long? ChangeId { get; set; }

        public CreditableEntityType Type { get; set; }

        public virtual Platform Platform { get; set; }

        public virtual ICollection<Content> Content { get; set; }
        public virtual ICollection<ContentCreditableEntity> ContentCreditableEntity { get; set; }

        public void Configure(EntityTypeBuilder<CreditableEntity> entity)
        {
            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IdOnPlatform).IsRequired();
            entity.HasIndex(e => e.IdOnPlatform).IsUnique();

            entity.Property(e => e.Name).IsRequired();

            entity.Property(e => e.ChangeId);
            entity.HasIndex(e => e.ChangeId).IsUnique();

            entity.Property(e => e.Type);

            entity.HasOne(d => d.Platform)
                .WithMany(p => p.CreditableEntities)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}