using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Utils;
using System;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Models
{
    public partial class SearchRequestV2 : BaseEntity, ITiming, IConfigurable<SearchRequestV2>
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public bool? LinkCreated { get; set; }

        public string From { get; set; }

        public string UserAgent { get; set; }

        public string Exception { get; set; }

        public FileFormatConstant? Format { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTime StartedAt { get; set; }

        public int Sequence { get; set; }

        public int? Flush { get; set; }

        public int? AreaCheck { get; set; }

        public int? Compute1024Red { get; set; }
        public int? Compute1024Green { get; set; }
        public int? Compute1024Blue { get; set; }
        public int? Compute1024Average { get; set; }

        public int? Compute256Red { get; set; }
        public int? Compute256Green { get; set; }
        public int? Compute256Blue { get; set; }
        public int? Compute256Average { get; set; }

        public int? Compute64Average { get; set; }

        public int? CompareCoarse { get; set; }
        public int? ReduceCoarseResults { get; set; }
        public int? RetrieveImageInfo { get; set; }
        public int? CompareGranular { get; set; }
        public int? ReduceGranularResults { get; set; }
        public int? CleanViewLocation { get; set; }
        public int? RetrieveCreditableEntities { get; set; }
        public int? AppendCreditableEntities { get; set; }
        public int? DetermineFinalOrder { get; set; }

        public int? LinkCreationPreparation { get; set; }
        public int? EnqueueLinkCreation { get; set; }
        public int? Finish { get; set; }

        public int? Count { get; set; }
        public int? UnlikelyCount { get; set; }
        public int? AlternativeCount { get; set; }
        public int? TossUpCount { get; set; }
        public int? ExactCount { get; set; }

        public void Configure(EntityTypeBuilder<SearchRequestV2> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Version).IsRequired();

            entity.Property(e => e.LinkCreated);
            entity.HasIndex(e => e.LinkCreated);

            entity.Property(e => e.From);
            entity.Property(e => e.UserAgent).IsRequired();

            entity.Property(e => e.Exception);

            entity.Property(e => e.Format);
            entity.Property(e => e.Width);
            entity.Property(e => e.Height);

            entity.Property(e => e.StartedAt);
            entity.Property(e => e.Sequence);

            entity.Property(e => e.Flush);
            entity.Property(e => e.AreaCheck);

            entity.Property(e => e.Compute1024Red);
            entity.Property(e => e.Compute1024Green);
            entity.Property(e => e.Compute1024Blue);
            entity.Property(e => e.Compute1024Average);

            entity.Property(e => e.Compute256Red);
            entity.Property(e => e.Compute256Green);
            entity.Property(e => e.Compute256Blue);
            entity.Property(e => e.Compute256Average);

            entity.Property(e => e.Compute64Average);

            entity.Property(e => e.CompareCoarse);
            entity.Property(e => e.ReduceCoarseResults);
            entity.Property(e => e.RetrieveImageInfo);
            entity.Property(e => e.CompareGranular);
            entity.Property(e => e.ReduceGranularResults);
            entity.Property(e => e.CleanViewLocation);
            entity.Property(e => e.RetrieveCreditableEntities);
            entity.Property(e => e.AppendCreditableEntities);
            entity.Property(e => e.DetermineFinalOrder);

            entity.Property(e => e.LinkCreationPreparation);
            entity.Property(e => e.EnqueueLinkCreation);
            entity.Property(e => e.Finish);

            entity.Property(e => e.Count);
            entity.Property(e => e.UnlikelyCount);
            entity.Property(e => e.AlternativeCount);
            entity.Property(e => e.TossUpCount);
            entity.Property(e => e.ExactCount);
        }
    }
}
