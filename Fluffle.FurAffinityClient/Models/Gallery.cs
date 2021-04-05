using System.Collections.Generic;

namespace Noppes.Fluffle.FurAffinity
{
    public class FaGallery
    {
        public string ArtistId { get; set; }

        public int Page { get; set; }

        public ICollection<FaFolder> Folders { get; set; }

        public ICollection<int> SubmissionIds { get; set; }

        public bool HasNextPage { get; set; }
    }

    public class FaFolder
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string NormalizedTitle { get; set; }
    }
}
