using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Feeder.Framework.Ingestion;

public class GroupedPutItemActionModelBuilder
{
    private readonly string _groupId;
    private readonly List<PutIndexItemActionModelBuilder> _builders;

    public GroupedPutItemActionModelBuilder(string groupId)
    {
        _groupId = groupId;
        _builders = [];
    }

    public PutIndexItemActionModelBuilder AddItem()
    {
        var builder = new PutIndexItemActionModelBuilder();
        _builders.Add(builder);

        return builder;
    }

    public ICollection<PutItemActionModel> Build()
    {
        if (_builders.Count == 0)
        {
            var deleteItemAction = new PutDeleteGroupItemActionModelBuilder()
                .WithGroupId(_groupId)
                .Build();

            return [deleteItemAction];
        }

        var groupItemIds = _builders.Select(x => x.GetItemId()).ToList();

        var itemActions = _builders
            .Select(PutItemActionModel (builder) => builder.WithGroup(_groupId, groupItemIds).Build())
            .ToList();

        return itemActions;
    }
}
