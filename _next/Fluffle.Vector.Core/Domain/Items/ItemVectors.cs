namespace Fluffle.Vector.Core.Domain.Items;

public class ItemVectors
{
    public required ItemVectorsId ItemVectorsId { get; set; }

    public required ICollection<ItemVector> Vectors { get; set; }
}

public class ItemVector
{
    public required float[] Value { get; set; }
}

public class ItemVectorsId
{
    public string ItemId { get; set; }

    public string ModelId { get; set; }

    public ItemVectorsId(string itemId, string modelId)
    {
        ItemId = itemId;
        ModelId = modelId;
    }
}
