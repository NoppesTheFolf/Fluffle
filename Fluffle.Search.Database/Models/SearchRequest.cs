using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Utils;
using System;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Models
{
    public partial class SearchRequest : BaseEntity, ITiming, IConfigurable<SearchRequest>
    {
        public int Id { get; set; }

        public string From { get; set; }

        public string UserAgent { get; set; }

        public string Exception { get; set; }

        public FileFormatConstant? Format { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTime StartedAt { get; set; }

        public int Sequence { get; set; }

        public int? Count { get; set; }

        public int? Flush { get; set; }

        public int? AreaCheck { get; set; }

        public int? Start256RgbComputation { get; set; }

        public int? Compute256Red { get; set; }

        public int? Compute256Green { get; set; }

        public int? Compute256Blue { get; set; }

        public int? Compute64Average { get; set; }

        public int? Compare64Average { get; set; }

        public int? ComplementComparisonResults { get; set; }

        public int? WaitFor256RgbComputation { get; set; }

        public int? CreateAndRefineOutput { get; set; }

        public void Configure(EntityTypeBuilder<SearchRequest> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.From);
            entity.Property(e => e.UserAgent).IsRequired();

            entity.Property(e => e.Exception);

            entity.Property(e => e.Format);
            entity.Property(e => e.Width);
            entity.Property(e => e.Height);

            entity.Property(e => e.StartedAt);
            entity.Property(e => e.Sequence);

            entity.Property(e => e.Count);

            entity.Property(e => e.Flush);
            entity.Property(e => e.AreaCheck);
            entity.Property(e => e.Start256RgbComputation);
            entity.Property(e => e.Compute256Red);
            entity.Property(e => e.Compute256Green);
            entity.Property(e => e.Compute256Blue);
            entity.Property(e => e.Compute64Average);
            entity.Property(e => e.Compare64Average);
            entity.Property(e => e.ComplementComparisonResults);
            entity.Property(e => e.WaitFor256RgbComputation);
            entity.Property(e => e.CreateAndRefineOutput);
        }
    }
}
