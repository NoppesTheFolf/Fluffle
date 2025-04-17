namespace Fluffle.Ingestion.Core.Domain.ItemActions;

public abstract class ItemAction
{
    public string? ItemActionId { get; set; }

    public required string ItemId { get; set; }

    public required int Priority { get; set; }

    public required int AttemptCount { get; set; }

    public required DateTime VisibleWhen { get; set; }

    public abstract T Visit<T>(IItemActionVisitor<T> visitor);
}
