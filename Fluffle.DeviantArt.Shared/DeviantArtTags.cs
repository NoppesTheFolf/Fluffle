using System.Collections;

namespace Noppes.Fluffle.DeviantArt.Shared;

public class DeviantArtTag
{
    public string Name { get; set; }

    public bool IsFurry { get; set; }

    public DeviantArtTag(string name, bool isFurry)
    {
        Name = name;
        IsFurry = isFurry;
    }
}

public class DeviantArtTags : IEnumerable<DeviantArtTag>
{
    private readonly IDictionary<string, bool> _tags;

    public DeviantArtTags(IEnumerable<string> furryTags, IEnumerable<string> generalTags)
    {
        _tags = furryTags.Select(x => (name: x, isFurry: true))
            .Concat(generalTags.Select(x => (name: x, isFurry: false)))
            .ToDictionary(x => x.name, x => x.isFurry, StringComparer.OrdinalIgnoreCase);
    }

    public bool? IsFurry(string name)
    {
        if (_tags.TryGetValue(name, out var value))
            return value;

        return null;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<DeviantArtTag> GetEnumerator() => _tags.Select(item => new DeviantArtTag(item.Key, item.Value)).GetEnumerator();
}