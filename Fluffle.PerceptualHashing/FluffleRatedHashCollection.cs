using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.PerceptualHashing;

/// <summary>
/// Basically the same as <see cref="FluffleHashCollection"/>, but makes a difference between
/// SFW and NSFW images.
/// </summary>
public class FluffleRatedHashCollection
{
    private readonly FluffleHashCollection _sfwHashes;
    private readonly FluffleHashCollection _nsfwHashes;

    public FluffleRatedHashCollection()
    {
        _sfwHashes = new FluffleHashCollection();
        _nsfwHashes = new FluffleHashCollection();
    }

    public bool Add(HashedImage image, bool isSfw)
    {
        var hashes = isSfw ? _sfwHashes : _nsfwHashes;

        return hashes.Add(image);
    }

    public bool Remove(int imageId)
    {
        return _sfwHashes.Remove(imageId) || _nsfwHashes.Remove(imageId);
    }

    public IEnumerable<Memory<HashedImage>> AsMemories(bool sfwOnly)
    {
        if (!sfwOnly)
            yield return _nsfwHashes.AsMemory();

        yield return _sfwHashes.AsMemory();
    }
}
