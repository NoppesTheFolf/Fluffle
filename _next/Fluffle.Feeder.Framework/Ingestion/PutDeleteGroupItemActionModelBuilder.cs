using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Feeder.Framework.Ingestion;

public class PutDeleteGroupItemActionModelBuilder
{
    private string? _groupId;

    public PutDeleteGroupItemActionModelBuilder WithGroupId(string groupId)
    {
        _groupId = groupId;

        return this;
    }

    public PutDeleteGroupItemActionModel Build()
    {
        if (string.IsNullOrWhiteSpace(_groupId)) throw new InvalidOperationException("Group ID has not been set.");

        return new PutDeleteGroupItemActionModel
        {
            GroupId = _groupId
        };
    }
}
