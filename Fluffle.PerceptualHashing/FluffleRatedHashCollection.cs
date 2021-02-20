using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.PerceptualHashing
{
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

        public bool Remove(int imageId, out RemovedImage image)
        {
            image = default;

            if (_sfwHashes.Remove(imageId, out var removedImage))
            {
                image = new RemovedImage(removedImage, true);
                return true;
            }

            if (_nsfwHashes.Remove(imageId, out removedImage))
            {
                image = new RemovedImage(removedImage, false);
                return true;
            }

            return false;
        }

        public IEnumerable<Memory<HashedImage>> AsMemories(bool sfwOnly)
        {
            if (!sfwOnly)
                yield return _nsfwHashes.AsMemory();

            yield return _sfwHashes.AsMemory();
        }
    }
}
