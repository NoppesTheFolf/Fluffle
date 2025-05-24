using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Models.Items;
using System.Text.Json;

namespace Fluffle.Feeder.Framework.Ingestion;

public class PutIndexItemActionModelBuilder
{
    private string? _itemId;
    private long? _priority;
    private readonly List<ImageModel> _images = [];
    private string? _url;
    private bool? _isSfw;
    private readonly List<FeederAuthor> _authors = [];

    public PutIndexItemActionModelBuilder WithItemId(string itemId)
    {
        _itemId = itemId;

        return this;
    }

    public PutIndexItemActionModelBuilder WithPriority(DateTimeOffset createdWhen)
    {
        return WithPriority(createdWhen.ToUnixTimeSeconds());
    }

    public PutIndexItemActionModelBuilder WithPriority(long priority)
    {
        _priority = priority;

        return this;
    }

    public PutIndexItemActionModelBuilder WithImage(int width, int height, string url)
    {
        _images.Add(new ImageModel
        {
            Width = width,
            Height = height,
            Url = url
        });

        return this;
    }

    public PutIndexItemActionModelBuilder WithUrl(string url)
    {
        _url = url;

        return this;
    }

    public PutIndexItemActionModelBuilder WithIsSfw(bool isSfw)
    {
        _isSfw = isSfw;

        return this;
    }

    public PutIndexItemActionModelBuilder WithAuthor(string id, string name)
    {
        _authors.Add(new FeederAuthor
        {
            Id = id,
            Name = name
        });

        return this;
    }

    public PutIndexItemActionModel Build()
    {
        if (string.IsNullOrWhiteSpace(_itemId)) throw new InvalidOperationException("Item ID has not been set.");
        if (_priority == null) throw new InvalidOperationException("Priority has not been set.");
        if (_images.Count == 0) throw new InvalidOperationException("No images have been added.");
        if (string.IsNullOrWhiteSpace(_url)) throw new InvalidOperationException("URL has not been set.");
        if (_isSfw == null) throw new InvalidOperationException("Whether the item is SFW has not been set.");

        return new PutIndexItemActionModel
        {
            ItemId = _itemId,
            Priority = _priority.Value,
            Images = _images,
            Properties = JsonSerializer.SerializeToNode(new FeederProperties
            {
                Url = _url,
                IsSfw = _isSfw,
                Authors = _authors
            }, JsonSerializerOptions.Web)!
        };
    }
}
