using MessagePack;
using Noppes.Fluffle.Constants;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    [MessagePackObject]
    public class UnprocessedContentModel
    {
        [Key(0)]
        public int ContentId { get; set; }

        [Key(1)]
        public PlatformConstant Platform { get; set; }

        [Key(2)]
        public string PlatformName { get; set; }

        [Key(3)]
        public string IdOnPlatform { get; set; }

        [MessagePackObject]
        public class FileModel
        {
            [Key(0)]
            public int Width { get; set; }

            [Key(1)]
            public int Height { get; set; }

            [Key(2)]
            public string Location { get; set; }
        }

        [Key(4)]
        public IEnumerable<FileModel> Files { get; set; }
    }

    [MessagePackObject]
    public class UnprocessedImageModel : UnprocessedContentModel
    {
        [Key(5)]
        public bool HasTransparency { get; set; }
    }
}
