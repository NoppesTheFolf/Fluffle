using Fluffle.Vector.Api.Client;
using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Api.Models.Vectors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text.Json;

namespace Fluffle.Vector.Api.IntegrationTests;

[TestFixture]
public class Tests
{
    private ServiceProvider _serviceProvider;
    private IVectorApiClient _vectorApiClient;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("VectorApiClient:Url", "http://127.0.0.1:1080"),
                    new KeyValuePair<string, string?>("VectorApiClient:ApiKey", "Lahqu3ReiMu4ouvooveerahpiu7Yahpo")
                ])
                .Build())
            .AddVectorApiClient()
            .BuildServiceProvider();

        _vectorApiClient = _serviceProvider.GetRequiredService<IVectorApiClient>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    [Test, Order(1)]
    public async Task Test01_GetItem_NonExistentItem()
    {
        var item = await _vectorApiClient.GetItemAsync("non-existent-id");
        item.ShouldBeNull();
    }

    [Test, Order(2)]
    public async Task Test02_PutItem_NewItem()
    {
        await _vectorApiClient.PutItemAsync("test-id", new PutItemModel
        {
            Images = new List<ImageModel>
            {
                new()
                {
                    Width = 123,
                    Height = 321,
                    Url = "https://fluffle.xyz/abc"
                },
                new()
                {
                    Width = 321,
                    Height = 123,
                    Url = "https://fluffle.xyz/def"
                }
            },
            Properties = JsonSerializer.SerializeToNode(new
            {
                ValueString = "string",
                ValueInt = 123,
                ValueArray = (int[])[1, 2, 3],
                ValueObject = new
                {
                    ValueObject = new
                    {
                        ValueString = "string"
                    }
                }
            }, JsonSerializerOptions.Web)!
        });
    }

    [Test, Order(3)]
    public async Task Test03_GetItem_ItemExists()
    {
        var item = await _vectorApiClient.GetItemAsync("test-id");

        var itemSerialized = JsonSerializer.Serialize(item, JsonSerializerOptions.Web);
        var expectedItemSerialized = """
                                     {
                                         "itemId": "test-id",
                                         "images": [
                                             {
                                                 "width": 123,
                                                 "height": 321,
                                                 "url": "https://fluffle.xyz/abc"
                                             },
                                             {
                                                 "width": 321,
                                                 "height": 123,
                                                 "url": "https://fluffle.xyz/def"
                                             }
                                         ],
                                         "properties": {
                                             "valueString": "string",
                                             "valueInt": 123,
                                             "valueArray": [
                                                 1,
                                                 2,
                                                 3
                                             ],
                                             "valueObject": {
                                                 "valueObject": {
                                                     "valueString": "string"
                                                 }
                                             }
                                         }
                                     }
                                     """.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty);

        itemSerialized.ShouldBe(expectedItemSerialized);
    }

    [Test, Order(4)]
    public async Task Test04_PutItemVectors_NonExistentItem()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("non-existent-id", "exactMatchV1",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = CreateRandomVector(1),
                    Properties = null
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No item with ID 'non-existent-id' could be found.");
    }

    [Test, Order(5)]
    public async Task Test05_PutItemVectors_NonExistentModel()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("test-id", "non-existent-model",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = CreateRandomVector(1),
                    Properties = null
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No model with ID 'non-existent-model' could be found.");
    }

    [Test, Order(6)]
    public async Task Test06_PutItemVectors_InvalidVectorLength()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("test-id", "exactMatchV1",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = CreateRandomVector(32),
                    Properties = null
                },
                new()
                {
                    Value = CreateRandomVector(1),
                    Properties = null
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("Query length of at least one vector does not equal expected vector length of model (32).");
    }

    [Test, Order(7)]
    public async Task Test07_PutItemVectors_NewItemVector()
    {
        await _vectorApiClient.PutItemVectorsAsync("test-id", "exactMatchV1", new List<PutItemVectorModel>
        {
            new()
            {
                Value = CreateRandomVector(32),
                Properties = null
            },
            new()
            {
                Value = CreateRandomVector(32),
                Properties = JsonSerializer.SerializeToNode(new
                {
                    ValueString = "string",
                    ValueInt = 123
                })
            }
        });
    }

    [Test, Order(8)]
    public async Task Test08_SearchVectorsAsync_NonExistentModel()
    {
        var act = _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "non-existent-model",
            Query = CreateRandomVector(1),
            Limit = 10
        });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No model with ID 'non-existent-model' could be found.");
    }

    [Test, Order(9)]
    public async Task Test09_SearchVectorsAsync_InvalidVectorLength()
    {
        var act = _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "exactMatchV1",
            Query = CreateRandomVector(1),
            Limit = 10
        });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("Query length of vector does not equal expected vector length of model (32).");
    }

    [Test, Order(10)]
    public async Task Test10_SearchVectorsAsync_CreatedItemReturned()
    {
        var results = await _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "exactMatchV1",
            Query = CreateRandomVector(32),
            Limit = 1
        });

        results.Count.ShouldBe(1);
        results[0].ItemId.ShouldBe("test-id");
        results[0].Distance.ShouldBeInRange(0, 1);
    }

    [Test, Order(11)]
    public async Task Test11_DeleteItem_NonExistentItem()
    {
        var act = _vectorApiClient.DeleteItemAsync("non-existent-id");

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No item with ID 'non-existent-id' could be found.");
    }

    [Test, Order(12)]
    public async Task Test12_DeleteItem_ItemExists()
    {
        var existingItem = await _vectorApiClient.GetItemAsync("test-id");
        existingItem.ShouldNotBeNull();

        await _vectorApiClient.DeleteItemAsync("test-id");

        var deletedItem = await _vectorApiClient.GetItemAsync("test-id");
        deletedItem.ShouldBeNull();

        var searchResults = await _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "exactMatchV1",
            Query = CreateRandomVector(32),
            Limit = 1
        });
        searchResults.ShouldBeEmpty();
    }

    private static float[] CreateRandomVector(int length)
    {
        var vector = new float[length];
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)Random.Shared.NextDouble();
        }

        return vector;
    }
}
