using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Domain.Vectors;
using Fluffle.Vector.Core.Repositories;
using Nito.AsyncEx;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        await _client.DeleteAsync(model.Id, Conditions.MatchKeyword("itemId", item.ItemId), wait: true);

        var points = vectors.Select(x =>
        {
            var point = new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = x.Value
            };

            point.Payload.Add("itemId", item.ItemId);

            var propertiesSerialized = JsonSerializer.Serialize(x.Properties);
            if (propertiesSerialized != "{}")
                point.Payload.Add("properties", propertiesSerialized);

            return point;
        }).ToList();
        await _client.UpsertAsync(model.Id, points, wait: true);
    }

    public async Task<ICollection<string>> GetCollectionsAsync(string itemId)
    {
        var foundCollections = new List<string>();

        var collections = await _client.ListCollectionsAsync();
        foreach (var collection in collections)
        {
            var count = await _client.CountAsync(collection, Conditions.MatchKeyword("itemId", itemId));
            if (count > 0)
            {
                foundCollections.Add(collection);
            }
        }

        return foundCollections;
    }

    public async Task<IList<VectorSearchResult>> GetAsync(string collectionId, float[] query, int limit)
    {
        var results = await _client.QueryAsync(
            collectionName: collectionId,
            query: query,
            limit: (ulong)limit
        );

        return results.Select(x =>
        {
            JsonNode properties = new JsonObject();
            if (x.Payload.TryGetValue("properties", out var propertiesValue))
            {
                properties = JsonSerializer.Deserialize<JsonNode>(propertiesValue.StringValue)!;
            }

            return new VectorSearchResult
            {
                ItemId = x.Payload["itemId"].StringValue,
                Distance = x.Score,
                Properties = properties
            };
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
