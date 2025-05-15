using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Domain.Vectors;
using Fluffle.Vector.Core.Repositories;
using Nito.AsyncEx;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;

namespace Fluffle.Vector.Qdrant;

internal class QdrantItemVectorsRepository : IItemVectorsRepository
{
    private readonly QdrantClient _client;
    private readonly HashSet<string> _initialized = [];
    private readonly AsyncLock _initializeLock = new();

    public QdrantItemVectorsRepository(QdrantClient client)
    {
        _client = client;
    }

    public async Task UpsertAsync(Model model, Item item, ICollection<ItemVector> vectors)
    {
        await EnsureCreatedAsync(model);

        var count = (int)await _client.CountAsync(model.Id, Conditions.MatchKeyword("itemId", item.ItemId));
        if (count == vectors.Count)
        {
            return;
        }

        if (count > 0)
        {
            await _client.DeleteAsync(model.Id, Conditions.MatchKeyword("itemId", item.ItemId), wait: true);
        }

        var points = vectors.Select(x =>
        {
            var point = new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = x.Value
            };

            point.Payload.Add("itemId", item.ItemId);

            if (x.Properties != null)
                point.Payload.Add("properties", JsonSerializer.Serialize(x.Properties));

            return point;
        }).ToList();
        await _client.UpsertAsync(model.Id, points, wait: true);
    }

    public async Task<IList<VectorSearchResult>> GetAsync(string modelId, float[] query, int limit)
    {
        var results = await _client.QueryAsync(
            collectionName: modelId,
            query: query,
            limit: (ulong)limit
        );

        return results.Select(x => new VectorSearchResult
        {
            ItemId = x.Payload["itemId"].StringValue,
            Distance = x.Score
        }).ToList();
    }

    public async Task DeleteAsync(string itemId)
    {
        var collections = await _client.ListCollectionsAsync();
        foreach (var collection in collections)
        {
            await _client.DeleteAsync(collection, Conditions.MatchKeyword("itemId", itemId), wait: true);
        }
    }

    public async Task EnsureCreatedAsync(Model model)
    {
        using var _ = await _initializeLock.LockAsync();

        if (_initialized.Contains(model.Id))
        {
            return;
        }

        if (!await _client.CollectionExistsAsync(model.Id))
        {
            await _client.CreateCollectionAsync(
                collectionName: model.Id,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)model.VectorDimensions,
                    Distance = Distance.Cosine,
                    OnDisk = true
                },
                onDiskPayload: true,
                hnswConfig: new HnswConfigDiff
                {
                    OnDisk = true
                },
                quantizationConfig: new QuantizationConfig
                {
                    Scalar = new ScalarQuantization
                    {
                        Type = QuantizationType.Int8,
                        Quantile = 1.0f,
                        AlwaysRam = false
                    }
                }
            );

            await _client.CreatePayloadIndexAsync(
                collectionName: model.Id,
                fieldName: "itemId",
                schemaType: PayloadSchemaType.Keyword,
                indexParams: new PayloadIndexParams
                {
                    KeywordIndexParams = new KeywordIndexParams
                    {
                        OnDisk = true
                    }
                },
                wait: true
            );
        }

        _initialized.Add(model.Id);
    }
}
