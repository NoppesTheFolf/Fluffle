using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Repositories;
using Fluffle.Vector.Core.Vectors;

namespace Fluffle.Vector.Core.Services;

public class ItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IItemVectorsRepository _itemVectorsRepository;
    private readonly VectorCollection _vectorCollection;

    public ItemService(IItemRepository itemRepository, IItemVectorsRepository itemVectorsRepository, VectorCollection vectorCollection)
    {
        _itemRepository = itemRepository;
        _itemVectorsRepository = itemVectorsRepository;
        _vectorCollection = vectorCollection;
    }

    public async Task DeleteAsync(string itemId)
    {
        await _itemRepository.DeleteAsync(itemId);
        _vectorCollection.Remove(itemId);
    }

    public async Task UpsertVectorsAsync(ItemVectors itemVectors)
    {
        if (itemVectors.Vectors.Count > 1)
        {
            throw new InvalidOperationException("Multiple vectors per item is not supported yet.");
        }

        await _itemVectorsRepository.UpsertAsync(itemVectors);
        foreach (var itemVector in itemVectors.Vectors)
        {
            _vectorCollection.Add(itemVectors.ItemVectorsId.ItemId, itemVector.Value);
        }
    }
}
