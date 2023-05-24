using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Constants;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Api.Helpers;

public class TagBlacklistCollection
{
    private readonly TagBlacklist _universalBlacklist;
    private readonly TagBlacklist _nsfwBlacklist;
    private readonly ILogger<TagBlacklistCollection> _logger;

    public TagBlacklistCollection(ILogger<TagBlacklistCollection> logger)
    {
        _universalBlacklist = new TagBlacklist();
        _nsfwBlacklist = new TagBlacklist();
        _logger = logger;
    }

    public void Initialize(IEnumerable<string> universalTags, IEnumerable<string> nsfwTags)
    {
        _universalBlacklist.Use(universalTags);
        _logger.LogInformation("Universal tag blacklist configured to use the following tags: {tags}", string.Join(", ", _universalBlacklist));

        _nsfwBlacklist.Use(nsfwTags);
        _logger.LogInformation("NSFW tag blacklist configured to use the following tags: {tags}", string.Join(", ", _nsfwBlacklist));
    }

    public bool Any(ICollection<string> tags, ContentRatingConstant rating)
    {
        if (_universalBlacklist.Any(tags))
            return true;

        return !rating.IsSfw() && _nsfwBlacklist.Any(tags);
    }
}
