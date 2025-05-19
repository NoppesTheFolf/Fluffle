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
        var item = await _vectorApiClient.GetItemAsync("nonExistentId");
        item.ShouldBeNull();
    }

    [Test, Order(2)]
    public async Task Test02_PutItem_NewItem()
    {
        await _vectorApiClient.PutItemAsync("testId", new PutItemModel
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
    public async Task Test03_GetItemAndGetItems_ItemExists()
    {
        var item = await _vectorApiClient.GetItemAsync("testId");

        var itemSerialized = JsonSerializer.Serialize(item, JsonSerializerOptions.Web);
        var expectedItemSerialized = """
                                     {
                                         "itemId": "testId",
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

        var items = await _vectorApiClient.GetItemsAsync(["something", "testId", "testId", "somethingElse"]);
        var itemsSerialized = JsonSerializer.Serialize(items, JsonSerializerOptions.Web);
        itemsSerialized.ShouldBe($"[{itemSerialized}]");
    }

    [Test, Order(4)]
    public async Task Test04_PutItemVectors_NonExistentItem()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("nonExistentId", "integrationTest",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = [0.1f, 0.2f],
                    Properties = null
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No item with ID 'nonExistentId' could be found.");
    }

    [Test, Order(5)]
    public async Task Test05_PutItemVectors_NonExistentModel()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("testId", "nonExistentModel",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = [0.1f, 0.2f],
                    Properties = null
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No model with ID 'nonExistentModel' could be found.");
    }

    [Test, Order(6)]
    public async Task Test06_PutItemVectors_InvalidVectorLength()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("testId", "integrationTest",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = [0.1f, 0.2f],
                    Properties = null
                },
                new()
                {
                    Value = [0.1f],
                    Properties = null
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("Query length of at least one vector does not equal expected vector length of model (2).");
    }

    [Test, Order(7)]
    public async Task Test07_PutItemVectors_NewItemVector()
    {
        await _vectorApiClient.PutItemVectorsAsync("testId", "integrationTest", new List<PutItemVectorModel>
        {
            new()
            {
                Value = [0.1f, 0.2f],
                Properties = null
            },
            new()
            {
                Value = [0.3f, 0.4f],
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
            ModelId = "nonExistentModel",
            Query = [0.1f, 0.2f],
            Limit = 10
        });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No model with ID 'nonExistentModel' could be found.");
    }

    [Test, Order(9)]
    public async Task Test09_SearchVectorsAsync_InvalidVectorLength()
    {
        var act = _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "integrationTest",
            Query = [0.1f],
            Limit = 10
        });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("Query length of vector does not equal expected vector length of model (2).");
    }

    [Test, Order(10)]
    public async Task Test10_SearchVectorsAsync_CreatedItemReturned()
    {
        var results = await _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "integrationTest",
            Query = [0.5f, 0.6f],
            Limit = 10
        });

        results.Count.ShouldBe(2);
        results[0].ItemId.ShouldBe("testId");
        results[0].Distance.ShouldBe(0.998f, 0.001f);
    }

    [Test, Order(11)]
    public async Task Test11_PutItemVectorsAndSearchVectors_ExistingItemVector()
    {
        await _vectorApiClient.PutItemVectorsAsync("testId", "integrationTest", new List<PutItemVectorModel>
        {
            new()
            {
                Value = [0.05f, 0.2f],
                Properties = null
            },
            new()
            {
                Value = [-0.05f, 0.2f],
                Properties = null
            }
        });

        var results = await _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "integrationTest",
            Query = [0.5f, 0.6f],
            Limit = 1
        });

        results.Count.ShouldBe(1);
        results[0].ItemId.ShouldBe("testId");
        results[0].Distance.ShouldBe(0.901f, 0.001f);
    }

    [Test, Order(12)]
    public async Task Test12_DeleteItem_NonExistentItem()
    {
        var act = _vectorApiClient.DeleteItemAsync("nonExistentId");

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.Message.ShouldBe("No item with ID 'nonExistentId' could be found.");
    }

    [Test, Order(13)]
    public async Task Test13_DeleteItem_ItemExists()
    {
        var existingItem = await _vectorApiClient.GetItemAsync("testId");
        existingItem.ShouldNotBeNull();

        await _vectorApiClient.DeleteItemAsync("testId");

        var deletedItem = await _vectorApiClient.GetItemAsync("testId");
        deletedItem.ShouldBeNull();

        var searchResults = await _vectorApiClient.SearchVectorsAsync(new VectorSearchParametersModel
        {
            ModelId = "integrationTest",
            Query = [0.1f, 0.2f],
            Limit = 1
        });
        searchResults.ShouldBeEmpty();
    }
}
