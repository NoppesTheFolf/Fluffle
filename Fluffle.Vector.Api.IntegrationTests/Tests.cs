using Fluffle.Vector.Api.Client;
using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Api.Models.Vectors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

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
                    new KeyValuePair<string, string?>("VectorApiClient:Url", "http://127.0.0.1:53966"),
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
    public async Task Test02_GetItemCollections_NonExistentItem()
    {
        var act = _vectorApiClient.GetItemCollectionsAsync("nonExistentId");

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        e.Message.ShouldBe("No item with ID 'nonExistentId' could be found.");
    }

    [Test, Order(3)]
    public async Task Test03_PutItem_NewItem()
    {
        await _vectorApiClient.PutItemAsync("testItemId", new PutItemModel
        {
            GroupId = "testGroupId",
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
            Thumbnail = new ThumbnailModel
            {
                Width = 100,
                Height = 200,
                CenterX = 30,
                CenterY = 40,
                Url = "https://fluffle.xyz/thumbnail"
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

    [Test, Order(4)]
    public async Task Test04_GetItemAndGetItems_ItemExists()
    {
        var item = await _vectorApiClient.GetItemAsync("testItemId");

        var itemSerialized = JsonSerializer.Serialize(item, JsonSerializerOptions.Web);
        var expectedItemSerialized = """
                                     {
                                         "itemId": "testItemId",
                                         "groupId": "testGroupId",
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
                                         "thumbnail": {
                                             "width": 100,
                                             "height": 200,
                                             "centerX": 30,
                                             "centerY": 40,
                                             "url": "https://fluffle.xyz/thumbnail"
                                         },
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

        var items = await _vectorApiClient.GetItemsAsync(itemIds: ["something", "testItemId", "testItemId", "somethingElse"], groupId: null);
        var itemsSerialized = JsonSerializer.Serialize(items, JsonSerializerOptions.Web);
        itemsSerialized.ShouldBe($"[{itemSerialized}]");

        items = await _vectorApiClient.GetItemsAsync(itemIds: null, groupId: "testGroupId");
        itemsSerialized = JsonSerializer.Serialize(items, JsonSerializerOptions.Web);
        itemsSerialized.ShouldBe($"[{itemSerialized}]");

        items = await _vectorApiClient.GetItemsAsync(itemIds: null, groupId: "nonExistentId");
        items.ShouldBeEmpty();
    }

    [Test, Order(5)]
    public async Task Test05_GetItemCollections_ItemExistsNoCollections()
    {
        var collections = await _vectorApiClient.GetItemCollectionsAsync("testItemId");
        collections.ShouldBeEmpty();
    }

    [Test, Order(6)]
    public async Task Test06_PutItemVectors_NonExistentItem()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("nonExistentId", "integrationTest",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = [0.1f, 0.2f],
                    Properties = new JsonObject()
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        e.Message.ShouldBe("No item with ID 'nonExistentId' could be found.");
    }

    [Test, Order(7)]
    public async Task Test07_PutItemVectors_NonExistentCollection()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("testItemId", "nonExistentCollection",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = [0.1f, 0.2f],
                    Properties = new JsonObject()
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        e.Message.ShouldBe("No collection with ID 'nonExistentCollection' could be found.");
    }

    [Test, Order(8)]
    public async Task Test08_PutItemVectors_InvalidVectorLength()
    {
        var act = _vectorApiClient.PutItemVectorsAsync("testItemId", "integrationTest",
            new List<PutItemVectorModel>
            {
                new()
                {
                    Value = [0.1f, 0.2f],
                    Properties = new JsonObject()
                },
                new()
                {
                    Value = [0.1f],
                    Properties = new JsonObject()
                }
            });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        e.Message.ShouldBe("Query length of at least one vector does not equal expected vector length of collection (2).");
    }

    [Test, Order(9)]
    public async Task Test09_PutItemVectors_NewItemVector()
    {
        await _vectorApiClient.PutItemVectorsAsync("testItemId", "integrationTest", new List<PutItemVectorModel>
        {
            new()
            {
                Value = [0.1f, 0.2f],
                Properties = new JsonObject()
            },
            new()
            {
                Value = [0.3f, 0.4f],
                Properties = JsonSerializer.SerializeToNode(new
                {
                    ValueString = "string",
                    ValueInt = 123
                })!
            }
        });
    }

    [Test, Order(10)]
    public async Task Test10_GetItemCollections_ItemExistsSingleCollection()
    {
        var collections = await _vectorApiClient.GetItemCollectionsAsync("testItemId");

        collections.Count.ShouldBe(1);
        collections.ShouldContain("integrationTest");
    }

    [Test, Order(11)]
    public async Task Test11_SearchVectorsAsync_NonExistentCollection()
    {
        var act = _vectorApiClient.SearchCollectionAsync("nonExistentCollection", new VectorSearchParametersModel
        {
            Query = [0.1f, 0.2f],
            Limit = 10
        });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        e.Message.ShouldBe("No collection with ID 'nonExistentCollection' could be found.");
    }

    [Test, Order(12)]
    public async Task Test12_SearchVectorsAsync_InvalidVectorLength()
    {
        var act = _vectorApiClient.SearchCollectionAsync("integrationTest", new VectorSearchParametersModel
        {
            Query = [0.1f],
            Limit = 10
        });

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        e.Message.ShouldBe("Query length of vector does not equal expected vector length of collection (2).");
    }

    [Test, Order(13)]
    public async Task Test13_SearchVectorsAsync_CreatedItemReturned()
    {
        var results = await _vectorApiClient.SearchCollectionAsync("integrationTest", new VectorSearchParametersModel
        {
            Query = [0.5f, 0.6f],
            Limit = 10
        });

        results.Count.ShouldBe(2);
        results[0].ItemId.ShouldBe("testItemId");
        results[0].Distance.ShouldBe(0.998f, 0.001f);
        var propertiesSerialized = JsonSerializer.Serialize(results[0].Properties);
        propertiesSerialized.ShouldBe("""{"ValueString":"string","ValueInt":123}""");
    }

    [Test, Order(14)]
    public async Task Test14_PutItemVectorsAndSearchVectors_ExistingItemVector()
    {
        await _vectorApiClient.PutItemVectorsAsync("testItemId", "integrationTest", new List<PutItemVectorModel>
        {
            new()
            {
                Value = [0.05f, 0.2f],
                Properties = new JsonObject()
            },
            new()
            {
                Value = [-0.05f, 0.2f],
                Properties = new JsonObject()
            }
        });

        var results = await _vectorApiClient.SearchCollectionAsync("integrationTest", new VectorSearchParametersModel
        {
            Query = [0.5f, 0.6f],
            Limit = 1
        });

        results.Count.ShouldBe(1);
        results[0].ItemId.ShouldBe("testItemId");
        results[0].Distance.ShouldBe(0.901f, 0.001f);
        var propertiesSerialized = JsonSerializer.Serialize(results[0].Properties);
        propertiesSerialized.ShouldBe("{}");
    }

    [Test, Order(15)]
    public async Task Test15_DeleteItem_NonExistentItem()
    {
        var act = _vectorApiClient.DeleteItemAsync("nonExistentId");

        var e = await act.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        e.Message.ShouldBe("No item with ID 'nonExistentId' could be found.");
    }

    [Test, Order(16)]
    public async Task Test16_DeleteItem_ItemExists()
    {
        var existingItem = await _vectorApiClient.GetItemAsync("testItemId");
        existingItem.ShouldNotBeNull();

        await _vectorApiClient.DeleteItemAsync("testItemId");

        var deletedItem = await _vectorApiClient.GetItemAsync("testItemId");
        deletedItem.ShouldBeNull();

        var getItemCollectionsAct = _vectorApiClient.GetItemCollectionsAsync("testItemId");
        var e = await getItemCollectionsAct.ShouldThrowAsync<VectorApiException>();
        e.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        e.Message.ShouldBe("No item with ID 'testItemId' could be found.");

        var searchResults = await _vectorApiClient.SearchCollectionAsync("integrationTest", new VectorSearchParametersModel
        {
            Query = [0.1f, 0.2f],
            Limit = 1
        });
        searchResults.ShouldBeEmpty();
    }
}
