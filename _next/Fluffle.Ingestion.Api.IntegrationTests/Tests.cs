using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Models.Items;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text.Json.Nodes;

namespace Fluffle.Ingestion.Api.IntegrationTests;

public class Tests
{
    private ServiceProvider _serviceProvider;
    private IIngestionApiClient _ingestionApiClient;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("IngestionApiClient:Url", "http://127.0.0.1:1080"),
                    new KeyValuePair<string, string?>("IngestionApiClient:ApiKey", "Lahqu3ReiMu4ouvooveerahpiu7Yahpo")
                ])
                .Build())
            .AddIngestionApiClient()
            .BuildServiceProvider();

        _ingestionApiClient = _serviceProvider.GetRequiredService<IIngestionApiClient>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    [Test, Order(1)]
    public async Task Test01_DequeueItemAction_NoItemActions()
    {
        var itemAction = await _ingestionApiClient.DequeueItemActionAsync();
        itemAction.ShouldBeNull();
    }

    [Test, Order(2)]
    public async Task Test02_AcknowledgeItemAction_NonExistentItemAction()
    {
        await _ingestionApiClient.AcknowledgeItemActionAsync("nonExistentId");
    }

    [Test, Order(3)]
    public async Task Test03_PutItemActions_Empty()
    {
        await _ingestionApiClient.PutItemActionsAsync([]);
    }

    [Test, Order(4)]
    public async Task Test04_PutAndDequeueItemActions_2IndexAnd1DeleteItemActions()
    {
        await _ingestionApiClient.PutItemActionsAsync(new List<PutItemActionModel>
        {
            new PutIndexItemActionModel
            {
                Priority = 1,
                ItemId = "itemId1",
                GroupId = null,
                GroupItemIds = null,
                Images = [new ImageModel
                {
                    Width = 123,
                    Height = 321,
                    Url = "https://fluffle.xyz/abc"
                }],
                Properties = new JsonObject()
            },
            new PutDeleteItemActionModel
            {
                ItemId = "itemId3"
            },
            new PutIndexItemActionModel
            {
                Priority = 2,
                ItemId = "itemId2",
                GroupId = "groupId2",
                GroupItemIds = ["itemId2"],
                Images = [new ImageModel
                {
                    Width = 321,
                    Height = 123,
                    Url = "https://fluffle.xyz/def"
                }],
                Properties = new JsonObject()
            }
        });

        var itemAction1 = await _ingestionApiClient.DequeueItemActionAsync();
        var deleteItemAction1 = itemAction1.ShouldBeOfType<DeleteItemActionModel>();
        deleteItemAction1.ItemActionId.ShouldNotBeNull();
        deleteItemAction1.ItemId.ShouldBe("itemId3");

        var itemAction2 = await _ingestionApiClient.DequeueItemActionAsync();
        var indexItemAction2 = itemAction2.ShouldBeOfType<IndexItemActionModel>();
        indexItemAction2.ItemActionId.ShouldNotBeNull();
        indexItemAction2.ItemId.ShouldBe("itemId2");

        var itemAction3 = await _ingestionApiClient.DequeueItemActionAsync();
        var indexItemAction3 = itemAction3.ShouldBeOfType<IndexItemActionModel>();
        indexItemAction3.ItemActionId.ShouldNotBeNull();
        indexItemAction3.ItemId.ShouldBe("itemId1");

        var itemAction4 = await _ingestionApiClient.DequeueItemActionAsync();
        itemAction4.ShouldBeNull();
    }

    [Test, Order(5)]
    public async Task Test05_PutAndDequeueItemActions_PutSameItemAction2Times()
    {
        var putItemAction = new PutDeleteItemActionModel
        {
            ItemId = Guid.NewGuid().ToString()
        };
        await _ingestionApiClient.PutItemActionsAsync([putItemAction]);
        await _ingestionApiClient.PutItemActionsAsync([putItemAction]);

        var dequeuedItemAction = await _ingestionApiClient.DequeueItemActionAsync();
        dequeuedItemAction.ShouldBeNull();
    }

    [Test, Order(6)]
    public async Task Test06_PutAndAcknowledgeAndDequeueItemAction_SingleItemAction()
    {
        var putItemAction = new PutDeleteItemActionModel
        {
            ItemId = Guid.NewGuid().ToString()
        };

        var createdItemActionIds = await _ingestionApiClient.PutItemActionsAsync([putItemAction]);
        createdItemActionIds.Count.ShouldBe(1);

        await _ingestionApiClient.AcknowledgeItemActionAsync(createdItemActionIds[0]);

        var dequeuedItemAction = await _ingestionApiClient.DequeueItemActionAsync();
        dequeuedItemAction.ShouldBeNull();
    }
}
