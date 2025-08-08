using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Worker.ItemActionHandlers;

public class ItemActionHandlerFactory : IItemActionModelVisitor<IItemActionHandler>
{
    private readonly IServiceProvider _serviceProvider;

    public ItemActionHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IItemActionHandler Visit(IndexItemActionModel model) => new IndexItemActionHandler(model, _serviceProvider);

    public IItemActionHandler Visit(DeleteItemActionModel model) => new DeleteItemActionHandler(model, _serviceProvider);

    public IItemActionHandler Visit(DeleteGroupItemActionModel model) => new DeleteGroupItemActionHandler(model, _serviceProvider);
}
