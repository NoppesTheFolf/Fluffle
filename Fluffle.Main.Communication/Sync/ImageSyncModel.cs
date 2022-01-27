using MessagePack;
using Noppes.Fluffle.Constants;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    [MessagePackObject]
    public class ImagesSyncModel : ITrackableModel<ImagesSyncModel.ImageModel>
    {
        [MessagePackObject]
        public class ImageModel
        {
            [Key(0)]
            public int Id { get; set; }

            [Key(1)]
            public int PlatformId { get; set; }

            [Key(2)]
            public string IdOnPlatform { get; set; }

            [Key(3)]
            public long ChangeId { get; set; }

            [Key(4)]
            public string ViewLocation { get; set; }

            [Key(5)]
            public bool IsDeleted { get; set; }

            [Key(6)]
            public bool IsSfw { get; set; }

            [MessagePackObject]
            public class HashModel
            {
                [Key(0)]
                public byte[] PhashRed64 { get; set; }

                [Key(1)]
                public byte[] PhashGreen64 { get; set; }

                [Key(2)]
                public byte[] PhashBlue64 { get; set; }

                [Key(3)]
                public byte[] PhashAverage64 { get; set; }

                [Key(4)]
                public byte[] PhashRed256 { get; set; }

                [Key(5)]
                public byte[] PhashGreen256 { get; set; }

                [Key(6)]
                public byte[] PhashBlue256 { get; set; }

                [Key(7)]
                public byte[] PhashAverage256 { get; set; }

                [Key(8)]
                public byte[] PhashRed1024 { get; set; }

                [Key(9)]
                public byte[] PhashGreen1024 { get; set; }

                [Key(10)]
                public byte[] PhashBlue1024 { get; set; }

                [Key(11)]
                public byte[] PhashAverage1024 { get; set; }
            }

            [Key(7)]
            public HashModel Hash { get; set; }

            [MessagePackObject]
            public class ThumbnailModel
            {
                [Key(0)]
                public int Width { get; set; }

                [Key(1)]
                public int CenterX { get; set; }

                [Key(2)]
                public int Height { get; set; }

                [Key(3)]
                public int CenterY { get; set; }

                [Key(4)]
                public string Location { get; set; }
            }

            [Key(8)]
            public ThumbnailModel Thumbnail { get; set; }

            [MessagePackObject]
            public class FileModel
            {
                [Key(0)]
                public FileFormatConstant Format { get; set; }

                [Key(1)]
                public int Width { get; set; }

                [Key(2)]
                public int Height { get; set; }

                [Key(3)]
                public string Location { get; set; }
            }

            [Key(9)]
            public IEnumerable<FileModel> Files { get; set; }

            [Key(10)]
            public IEnumerable<int> Credits { get; set; }
        }

        [Key(0)]
        public long NextChangeId { get; set; }

        [Key(1)]
        public ICollection<ImageModel> Results { get; set; }
    }
}
