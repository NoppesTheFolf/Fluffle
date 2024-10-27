using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Sync;

public interface IContentMapper<in T>
{
    public PutContentModel SrcToContent(T src)
    {
        var dest = new PutContentModel
        {
            Reference = GetReference(src),
            IdOnPlatform = GetId(src),
            Rating = GetRating(src),
            Title = GetTitle(src),
            Description = GetDescription(src),
            CreditableEntities = GetCredits(src)?.ToList(),
            ViewLocation = GetViewLocation(src),
            Files = GetFiles(src)?.ToList(),
            OtherSources = GetOtherSources(src)?.ToList(),
            MediaType = GetMediaType(src),
            HasTransparency = GetHasTransparency(src),
            Priority = GetPriority(src),
            ShouldBeIndexed = ShouldBeIndexed(src)
        };

        return dest;
    }

    public string GetId(T src);

    public string GetReference(T src);

    public ContentRatingConstant GetRating(T src);

    public IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(T src);

    public string GetViewLocation(T src);

    public IEnumerable<PutContentModel.FileModel> GetFiles(T src);

    public MediaTypeConstant GetMediaType(T src);

    public bool GetHasTransparency(T src);

    public int GetPriority(T src);

    public string GetTitle(T src);

    public string GetDescription(T src);

    public IEnumerable<string> GetOtherSources(T src);

    public bool ShouldBeIndexed(T src);
}
