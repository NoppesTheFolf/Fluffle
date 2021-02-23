using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Main.Api.Helpers
{
    public class TagBlacklist : IEnumerable<string>
    {
        private readonly HashSet<string> _tags;

        public TagBlacklist()
        {
            _tags = new HashSet<string>();
        }

        public void Use(IEnumerable<string> tags)
        {
            _tags.Clear();

            var normalizedTags = tags
                .Select(TagHelper.Normalize)
                .Distinct();

            foreach (var normalizedTag in normalizedTags)
                _tags.Add(normalizedTag);
        }

        public bool Any(IEnumerable<string> tags) => tags.Any(Contains);

        public bool Contains(string tag) => _tags.Contains(TagHelper.Normalize(tag));

        public IEnumerator<string> GetEnumerator() => _tags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_tags).GetEnumerator();
    }
}
