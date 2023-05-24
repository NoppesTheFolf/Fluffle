using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Utils;
using System;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Models;

public partial class SearchRequest : BaseEntity, ITiming, IConfigurable<SearchRequest>
{
    public int Id { get; set; }

    public string QueryId { get; set; }

    public bool? LinkCreated { get; set; }

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

    public int? StartExpensiveRgbComputation { get; set; }

    public int? ComputeExpensiveRed { get; set; }

    public int? ComputeExpensiveGreen { get; set; }

    public int? ComputeExpensiveBlue { get; set; }

    public int? Compute64Average { get; set; }

    public int? Compare64Average { get; set; }

    public int? ComplementComparisonResults { get; set; }

    public int? WaitForExpensiveRgbComputation { get; set; }

    public int? CreateAndRefineOutput { get; set; }

    public void Configure(EntityTypeBuilder<SearchRequest> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(e => e.Id);

        entity.Property(e => e.QueryId);
        entity.HasIndex(e => e.QueryId).IsUnique();

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

        entity.Property(e => e.Count);

        entity.Property(e => e.Flush);
        entity.Property(e => e.AreaCheck);
        entity.Property(e => e.StartExpensiveRgbComputation);
        entity.Property(e => e.ComputeExpensiveRed);
        entity.Property(e => e.ComputeExpensiveGreen);
        entity.Property(e => e.ComputeExpensiveBlue);
        entity.Property(e => e.Compute64Average);
        entity.Property(e => e.Compare64Average);
        entity.Property(e => e.ComplementComparisonResults);
        entity.Property(e => e.WaitForExpensiveRgbComputation);
        entity.Property(e => e.CreateAndRefineOutput);
    }
}
