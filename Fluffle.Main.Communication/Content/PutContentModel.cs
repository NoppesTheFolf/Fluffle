using Noppes.Fluffle.Constants;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    public class PutContentModel
    {
        public string IdOnPlatform { get; set; }

        public string ViewLocation { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ContentRatingConstant Rating { get; set; }

        public MediaTypeConstant MediaType { get; set; }

        public ICollection<string> Tags { get; set; }

        public int Priority { get; set; }

        public class FileModel
        {
            public string Location { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public FileFormatConstant Format { get; set; }
        }

        public ICollection<FileModel> Files { get; set; }

        public class CreditableEntityModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public CreditableEntityType Type { get; set; }
        }

        public ICollection<CreditableEntityModel> CreditableEntities { get; set; }

        public byte[] Source { get; set; }

        public int? SourceVersion { get; set; }
    }
}
