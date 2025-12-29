using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Api.Models.Items;
using System.Text.Json;

namespace Fluffle.Feeder.Framework.Ingestion;

public class PutIndexItemActionModelBuilder
{
    private string? _itemId;
    private string? _groupId;
    private ICollection<string>? _groupItemIds;
    private DateTimeOffset? _createdWhen;
    private readonly List<ImageModel> _images = [];
    private bool _requireImageExtensionValidation = true;
    private string? _url;
    private bool? _isSfw;
    private readonly List<FeederAuthor> _authors = [];
    private bool _requireAuthors = true;

    public PutIndexItemActionModelBuilder WithItemId(string itemId)
    {
        _itemId = itemId;

        return this;
    }

    public string GetItemId()
    {
        if (_itemId == null) throw new InvalidOperationException("Item ID has not been set.");

        return _itemId;
    }

    public PutIndexItemActionModelBuilder WithGroup(string groupId, ICollection<string> groupItemIds)
    {
        _groupId = groupId;
        _groupItemIds = groupItemIds;

        return this;
    }

    public PutIndexItemActionModelBuilder WithCreatedWhen(DateTimeOffset createdWhen)
    {
        _createdWhen = createdWhen;

        return this;
    }

    public PutIndexItemActionModelBuilder WithImage(int width, int height, string url) =>
        WithImages([
            new ImageModel
            {
                Width = width,
                Height = height,
                Url = url
            }
        ]);

    public PutIndexItemActionModelBuilder WithImages(IEnumerable<ImageModel> images)
    {
        _images.AddRange(images);

        return this;
    }

    public PutIndexItemActionModelBuilder SkipImageExtensionValidation()
    {
        _requireImageExtensionValidation = false;

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

    public PutIndexItemActionModelBuilder WithAuthor(string id, string name) =>
        WithAuthors([
            new FeederAuthor
            {
                Id = id,
                Name = name
            }
        ]);

    public PutIndexItemActionModelBuilder WithAuthors(IEnumerable<FeederAuthor> authors)
    {
        _authors.AddRange(authors);

        return this;
    }

    public PutIndexItemActionModelBuilder AllowNoAuthors()
    {
        _requireAuthors = false;

        return this;
    }

    public PutIndexItemActionModel Build()
    {
        if (string.IsNullOrWhiteSpace(_itemId)) throw new InvalidOperationException("Item ID has not been set.");
        if (_createdWhen == null) throw new InvalidOperationException("Created when has not been set.");
        if (_images.Count == 0) throw new InvalidOperationException("No images have been added.");
        if (_requireImageExtensionValidation)
        {
            foreach (var image in _images)
            {
                var imageExtension = Path.GetExtension(image.Url);
                if (!ImageHelper.IsSupportedExtension(imageExtension))
                {
                    throw new InvalidOperationException($"Image has an unsupported extension: {image.Url}");
                }
            }
        }
        if (string.IsNullOrWhiteSpace(_url)) throw new InvalidOperationException("URL has not been set.");
        if (_isSfw == null) throw new InvalidOperationException("Whether the item is SFW has not been set.");
        if (_requireAuthors && _authors.Count == 0) throw new InvalidOperationException("No authors have been added.");
        foreach (var author in _authors)
        {
            if (string.IsNullOrWhiteSpace(author.Id))
            {
                throw new InvalidOperationException("Author ID should not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(author.Name))
            {
                throw new InvalidOperationException("Author name should not be null or whitespace.");
            }
        }

        return new PutIndexItemActionModel
        {
            ItemId = _itemId,
            GroupId = _groupId,
            GroupItemIds = _groupItemIds,
            Priority = _createdWhen.Value.ToUnixTimeSeconds(),
            Images = _images,
            Properties = JsonSerializer.SerializeToNode(new FeederProperties
            {
                Url = _url,
                IsSfw = _isSfw,
                Authors = _authors,
                CreatedWhen = _createdWhen.Value.UtcDateTime
            }, JsonSerializerOptions.Web)!
        };
    }
}
