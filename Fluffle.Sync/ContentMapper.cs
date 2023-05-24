using MessagePack;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

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
            Tags = GetTags(src)?.ToList(),
            OtherSources = GetOtherSources(src)?.ToList(),
            MediaType = GetMediaType(src),
            Priority = GetPriority(src),
            ShouldBeIndexed = ShouldBeIndexed(src)
        };

        if (typeof(T).GetCustomAttribute<MessagePackObjectAttribute>() == null)
            return dest;

        if (GetSourceVersion() < 1)
            throw new InvalidOperationException("Specify a source version greater than 0.");

        using var sourceStream = new MemoryStream();
        using (var compressionStream = new BrotliStream(sourceStream, CompressionLevel.Optimal))
            compressionStream.Write(MessagePackSerializer.Serialize(src));

        dest.Source = sourceStream.ToArray();
        dest.SourceVersion = GetSourceVersion();
        return dest;
    }

    public string GetId(T src);

    public string GetReference(T src);

    public ContentRatingConstant GetRating(T src);

    public IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(T src);

    public string GetViewLocation(T src);

    public IEnumerable<PutContentModel.FileModel> GetFiles(T src);

    public IEnumerable<string> GetTags(T src);

    public MediaTypeConstant GetMediaType(T src);

    public int GetPriority(T src);

    public string GetTitle(T src);

    public string GetDescription(T src);

    public IEnumerable<string> GetOtherSources(T src);

    public bool ShouldBeIndexed(T src);

    public int GetSourceVersion();
}
