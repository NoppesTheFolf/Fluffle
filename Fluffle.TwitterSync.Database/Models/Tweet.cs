using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class Tweet : BaseEntity, IConfigurable<Tweet>
    {
        public Tweet()
        {
            Media = new HashSet<Media>();
            TweetMedia = new HashSet<TweetMedia>();
            Mentions = new HashSet<UserMention>();
        }

        /// <summary>
        /// The ID of this tweet.
        /// </summary>
        public string Id { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// The amount of favorites this tweet has gotten.
        /// </summary>
        public int FavoriteCount { get; set; }

        /// <summary>
        /// The number of times this tweet has been retweeted.
        /// </summary>
        public int RetweetCount { get; set; }

        public string ReplyTweetId { get; set; }
        public string ReplyUserId { get; set; }

        public string QuotedTweetId { get; set; }

        public string RetweetTweetId { get; set; }

        public string CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public TweetType Type { get; set; }

        public bool ShouldBeAnalyzed { get; set; }
        public long ReservedUntil { get; set; }
        public DateTimeOffset? AnalyzedAt { get; set; }

        public virtual ICollection<Media> Media { get; set; }
        public virtual ICollection<TweetMedia> TweetMedia { get; set; }

        public virtual ICollection<UserMention> Mentions { get; set; }

        public void Configure(EntityTypeBuilder<Tweet> entity)
        {
            entity.Property(e => e.Id).HasMaxLength(20).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Url).HasMaxLength(256).IsRequired();

            entity.Property(e => e.FavoriteCount);
            entity.Property(e => e.RetweetCount);

            entity.Property(e => e.ReplyTweetId).HasMaxLength(20);
            entity.HasIndex(e => e.ReplyTweetId);

            entity.Property(e => e.ReplyUserId).HasMaxLength(20);
            entity.HasIndex(e => e.ReplyUserId);

            entity.Property(e => e.QuotedTweetId).HasMaxLength(20);
            entity.HasIndex(e => e.QuotedTweetId);

            entity.Property(e => e.RetweetTweetId).HasMaxLength(20);
            entity.HasIndex(e => e.RetweetTweetId);

            entity.Property(e => e.CreatedById).HasMaxLength(20).IsRequired();
            entity.HasOne(e => e.CreatedBy)
                .WithMany(e => e.Tweets)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.Type);

            entity.Property(e => e.ShouldBeAnalyzed);
            entity.Property(e => e.ReservedUntil);
            entity.Property(e => e.AnalyzedAt);

            entity.HasIndex(e => new { e.ShouldBeAnalyzed, e.AnalyzedAt, e.ReservedUntil, e.FavoriteCount });

            entity.HasMany(e => e.Media)
                .WithMany(e => e.Tweets)
                .UsingEntity<TweetMedia>(r =>
                {
                    return r.HasOne(e => e.Media)
                        .WithMany(e => e.TweetMedia)
                        .HasForeignKey(e => e.MediaId)
                        .OnDelete(DeleteBehavior.Restrict);
                }, l =>
                {
                    return l.HasOne(e => e.Tweet)
                        .WithMany(e => e.TweetMedia)
                        .HasForeignKey(e => e.TweetId)
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
