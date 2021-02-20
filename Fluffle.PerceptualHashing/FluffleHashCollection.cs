using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.PerceptualHashing
{
    /// <summary>
    /// A collection which can handle hashes somewhat efficiently.
    /// </summary>
    public class FluffleHashCollection
    {
        private const int ResizeStep = 1_000_000;

        public int Length { get; private set; }

        private HashedImage[] _hashes;
        private readonly HashSet<int> _imageIds;

        public FluffleHashCollection(int initSize = 0)
        {
            _hashes = new HashedImage[initSize];
            _imageIds = new HashSet<int>(initSize);
        }

        /// <summary>
        /// Adds the given image to the collection. If the image already exists, it's replaced
        /// instead and returns false. If the image got added, it returns true.
        /// </summary>
        public bool Add(HashedImage image)
        {
            var index = Find(image.Id);

            if (index != -1)
            {
                _hashes[index] = image;
                return false;
            }

            if (_hashes.Length == Length)
                Array.Resize(ref _hashes, Length + ResizeStep);

            _hashes[Length] = image;
            _imageIds.Add(image.Id);
            Length++;
            return true;
        }

        /// <summary>
        /// Removes the image with the given ID. Returns false if no image with said ID exists in
        /// the collection.
        /// </summary>
        public bool Remove(int imageId, out HashedImage removedImage)
        {
            removedImage = default;

            var foundIndex = Find(imageId);

            if (foundIndex == -1)
                return false;

            removedImage = RemoveAt(foundIndex);
            return true;
        }

        /// <summary>
        /// Removes the image at the given location.
        /// </summary>
        private HashedImage RemoveAt(int index)
        {
            var removed = _hashes[index];

            for (var i = index; i < Length - 1; i++)
                _hashes[i] = _hashes[i + 1];

            _imageIds.Remove(removed.Id);
            Length--;
            return removed;
        }

        /// <summary>
        /// Finds the index of the image with the given ID. Returns -1 if the collection does not
        /// contain the given image ID.
        /// </summary>
        private int Find(int imageId)
        {
            var foundIndex = -1;

            if (!_imageIds.Contains(imageId))
                return foundIndex;

            for (var i = 0; i < Length; i++)
            {
                if (_hashes[i].Id != imageId)
                    continue;

                foundIndex = i;
                break;
            }

            return foundIndex;
        }

        public Memory<HashedImage> AsMemory() => _hashes.AsMemory(0, Length);
    }
}
