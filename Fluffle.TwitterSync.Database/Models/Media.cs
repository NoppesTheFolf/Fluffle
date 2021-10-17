using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class Media : BaseEntity, IConfigurable<Media>
    {
        public Media()
        {
            Tweets = new HashSet<Tweet>();
            TweetMedia = new HashSet<TweetMedia>();
            Sizes = new HashSet<MediaSize>();
        }

        public string Id { get; set; }

        public MediaTypeConstant MediaType { get; set; }

        public string Url { get; set; }

        public bool? IsFurryArt { get; set; }

        public virtual ICollection<Tweet> Tweets { get; set; }
        public virtual ICollection<TweetMedia> TweetMedia { get; set; }

        public virtual ICollection<MediaSize> Sizes { get; set; }

        public virtual MediaAnalytic MediaAnalytic { get; set; }

        public void Configure(EntityTypeBuilder<Media> entity)
        {
            entity.Property(e => e.Id).HasMaxLength(20).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MediaType);

            entity.Property(e => e.Url).IsRequired().HasMaxLength(256);

            entity.Property(e => e.IsFurryArt);
        }
    }
}
