﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations
{
    [DbContext(typeof(TwitterContext))]
    [Migration("20211110204932_AddIsDeletedToMedia")]
    partial class AddIsDeletedToMedia
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:Collation", "en_US.utf8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.E621Artist", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_e621_artist");

                    b.ToTable("e621_artist");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.E621ArtistUrl", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<int>("ArtistId")
                        .HasColumnType("integer")
                        .HasColumnName("artist_id");

                    b.Property<bool?>("TwitterExists")
                        .HasColumnType("boolean")
                        .HasColumnName("twitter_exists");

                    b.Property<string>("TwitterUsername")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)")
                        .HasColumnName("twitter_username");

                    b.HasKey("Id")
                        .HasName("pk_e621_artist_url");

                    b.HasIndex("ArtistId")
                        .HasDatabaseName("idx_e621_artist_url_artist_id");

                    b.ToTable("e621_artist_url");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.Media", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("id");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool?>("IsFurryArt")
                        .HasColumnType("boolean")
                        .HasColumnName("is_furry_art");

                    b.Property<int>("MediaType")
                        .HasColumnType("integer")
                        .HasColumnName("media_type");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("url");

                    b.HasKey("Id")
                        .HasName("pk_media");

                    b.ToTable("media");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.MediaAnalytic", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("id");

                    b.Property<double>("Anime")
                        .HasColumnType("double precision")
                        .HasColumnName("anime");

                    b.Property<int[]>("ArtistIds")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasColumnName("artist_ids");

                    b.Property<double>("FurryArt")
                        .HasColumnType("double precision")
                        .HasColumnName("furry_art");

                    b.Property<double>("Fursuit")
                        .HasColumnType("double precision")
                        .HasColumnName("fursuit");

                    b.Property<double>("Real")
                        .HasColumnType("double precision")
                        .HasColumnName("real");

                    b.HasKey("Id")
                        .HasName("pk_media_analytic");

                    b.ToTable("media_analytic");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.MediaSize", b =>
                {
                    b.Property<string>("MediaId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("media_id");

                    b.Property<int>("Size")
                        .HasColumnType("integer")
                        .HasColumnName("size");

                    b.Property<int>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<int>("ResizeMode")
                        .HasColumnType("integer")
                        .HasColumnName("resize_mode");

                    b.Property<int>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("MediaId", "Size")
                        .HasName("pk_media_size");

                    b.ToTable("media_size");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.Tweet", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset?>("AnalyzedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("analyzed_at");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("created_by_id");

                    b.Property<int>("FavoriteCount")
                        .HasColumnType("integer")
                        .HasColumnName("favorite_count");

                    b.Property<string>("QuotedTweetId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("quoted_tweet_id");

                    b.Property<string>("ReplyTweetId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("reply_tweet_id");

                    b.Property<string>("ReplyUserId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("reply_user_id");

                    b.Property<long>("ReservedUntil")
                        .HasColumnType("bigint")
                        .HasColumnName("reserved_until");

                    b.Property<int>("RetweetCount")
                        .HasColumnType("integer")
                        .HasColumnName("retweet_count");

                    b.Property<string>("RetweetTweetId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("retweet_tweet_id");

                    b.Property<bool>("ShouldBeAnalyzed")
                        .HasColumnType("boolean")
                        .HasColumnName("should_be_analyzed");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("url");

                    b.HasKey("Id")
                        .HasName("pk_tweet");

                    b.HasIndex("CreatedById")
                        .HasDatabaseName("idx_tweet_created_by_id");

                    b.HasIndex("QuotedTweetId")
                        .HasDatabaseName("idx_tweet_quoted_tweet_id");

                    b.HasIndex("ReplyTweetId")
                        .HasDatabaseName("idx_tweet_reply_tweet_id");

                    b.HasIndex("ReplyUserId")
                        .HasDatabaseName("idx_tweet_reply_user_id");

                    b.HasIndex("RetweetTweetId")
                        .HasDatabaseName("idx_tweet_retweet_tweet_id");

                    b.HasIndex("ShouldBeAnalyzed", "AnalyzedAt", "ReservedUntil", "FavoriteCount")
                        .HasDatabaseName("idx_tweet_should_be_analyzed_and_analyzed_at_and_reserved_until_and_favorite_count");

                    b.ToTable("tweet");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.TweetMedia", b =>
                {
                    b.Property<string>("MediaId")
                        .HasColumnType("character varying(20)")
                        .HasColumnName("media_id");

                    b.Property<string>("TweetId")
                        .HasColumnType("character varying(20)")
                        .HasColumnName("tweet_id");

                    b.HasKey("MediaId", "TweetId")
                        .HasName("pk_tweet_media");

                    b.HasIndex("TweetId")
                        .HasDatabaseName("idx_tweet_media_tweet_id");

                    b.ToTable("tweet_media");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("id");

                    b.Property<int>("FollowersCount")
                        .HasColumnType("integer")
                        .HasColumnName("followers_count");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool?>("IsFurryArtist")
                        .HasColumnType("boolean")
                        .HasColumnName("is_furry_artist");

                    b.Property<bool>("IsOnE621")
                        .HasColumnType("boolean")
                        .HasColumnName("is_on_e621");

                    b.Property<bool>("IsProtected")
                        .HasColumnType("boolean")
                        .HasColumnName("is_protected");

                    b.Property<bool>("IsSuspended")
                        .HasColumnType("boolean")
                        .HasColumnName("is_suspended");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<long>("ReservedUntil")
                        .HasColumnType("bigint")
                        .HasColumnName("reserved_until");

                    b.Property<DateTimeOffset?>("TimelineRetrievedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timeline_retrieved_at");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_user");

                    b.HasIndex("IsFurryArtist", "IsOnE621", "IsProtected", "IsSuspended", "IsDeleted", "ReservedUntil", "FollowersCount")
                        .HasDatabaseName("idx_user_is_furry_artist_and_is_on_e621_and_is_protected_and_is_suspended_and_is_deleted_and_reserved_until_and_followers_count");

                    b.ToTable("user");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.UserMention", b =>
                {
                    b.Property<string>("TweetId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("tweet_id");

                    b.Property<string>("UserId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("user_id");

                    b.HasKey("TweetId", "UserId")
                        .HasName("pk_user_mention");

                    b.ToTable("user_mention");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.E621ArtistUrl", b =>
                {
                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.E621Artist", "Artist")
                        .WithMany("Urls")
                        .HasForeignKey("ArtistId")
                        .HasConstraintName("fk_e621_artist")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.MediaAnalytic", b =>
                {
                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.Media", "Media")
                        .WithOne("MediaAnalytic")
                        .HasForeignKey("Noppes.Fluffle.TwitterSync.Database.Models.MediaAnalytic", "Id")
                        .HasConstraintName("fk_media")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Media");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.MediaSize", b =>
                {
                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.Media", "Media")
                        .WithMany("Sizes")
                        .HasForeignKey("MediaId")
                        .HasConstraintName("fk_media")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Media");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.Tweet", b =>
                {
                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.User", "CreatedBy")
                        .WithMany("Tweets")
                        .HasForeignKey("CreatedById")
                        .HasConstraintName("fk_users")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.TweetMedia", b =>
                {
                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.Media", "Media")
                        .WithMany("TweetMedia")
                        .HasForeignKey("MediaId")
                        .HasConstraintName("fk_media")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.Tweet", "Tweet")
                        .WithMany("TweetMedia")
                        .HasForeignKey("TweetId")
                        .HasConstraintName("fk_tweet")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Media");

                    b.Navigation("Tweet");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.UserMention", b =>
                {
                    b.HasOne("Noppes.Fluffle.TwitterSync.Database.Models.Tweet", "Tweet")
                        .WithMany("Mentions")
                        .HasForeignKey("TweetId")
                        .HasConstraintName("fk_tweet")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tweet");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.E621Artist", b =>
                {
                    b.Navigation("Urls");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.Media", b =>
                {
                    b.Navigation("MediaAnalytic");

                    b.Navigation("Sizes");

                    b.Navigation("TweetMedia");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.Tweet", b =>
                {
                    b.Navigation("Mentions");

                    b.Navigation("TweetMedia");
                });

            modelBuilder.Entity("Noppes.Fluffle.TwitterSync.Database.Models.User", b =>
                {
                    b.Navigation("Tweets");
                });
#pragma warning restore 612, 618
        }
    }
}
