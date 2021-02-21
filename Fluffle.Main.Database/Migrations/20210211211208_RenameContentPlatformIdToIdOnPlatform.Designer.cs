﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Noppes.Fluffle.Main.Database.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    [DbContext(typeof(FluffleContext))]
    [Migration("20210211211208_RenameContentPlatformIdToIdOnPlatform")]
    partial class RenameContentPlatformIdToIdOnPlatform
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:Collation", "en_US.utf8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ApiKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("character varying")
                        .HasColumnName("description");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character(32)")
                        .HasColumnName("key")
                        .IsFixedLength(true);

                    b.HasKey("Id")
                        .HasName("pk_api_key");

                    b.HasIndex("Key")
                        .IsUnique()
                        .HasDatabaseName("uq_api_key_key");

                    b.ToTable("api_key");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ApiKeyPermission", b =>
                {
                    b.Property<int>("ApiKeyId")
                        .HasColumnType("integer")
                        .HasColumnName("api_key_id");

                    b.Property<int>("PermissionId")
                        .HasColumnType("integer")
                        .HasColumnName("permission_id");

                    b.HasKey("ApiKeyId", "PermissionId")
                        .HasName("pk_api_key_permission");

                    b.HasIndex("PermissionId")
                        .HasDatabaseName("idx_api_key_permission_permission_id");

                    b.ToTable("api_key_permission");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Content", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<long?>("ChangeId")
                        .HasColumnType("bigint")
                        .HasColumnName("change_id");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("discriminator");

                    b.Property<string>("IdOnPlatform")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("id_on_platform");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool>("IsIndexed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_indexed");

                    b.Property<int>("LastEditedById")
                        .HasColumnType("integer")
                        .HasColumnName("last_edited_by_id");

                    b.Property<int>("MediaTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("media_type_id");

                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int>("Priority")
                        .HasColumnType("integer")
                        .HasColumnName("priority");

                    b.Property<int>("RatingId")
                        .HasColumnType("integer")
                        .HasColumnName("rating_id");

                    b.Property<bool>("RequiresIndexing")
                        .HasColumnType("boolean")
                        .HasColumnName("requires_indexing");

                    b.Property<int?>("ThumbnailId")
                        .HasColumnType("integer")
                        .HasColumnName("thumbnail_id");

                    b.Property<string>("Title")
                        .HasColumnType("character varying")
                        .HasColumnName("title");

                    b.Property<string>("ViewLocation")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("view_location");

                    b.HasKey("Id")
                        .HasName("pk_content");

                    b.HasIndex("ChangeId")
                        .IsUnique()
                        .HasDatabaseName("uq_content_change_id");

                    b.HasIndex("IdOnPlatform")
                        .HasDatabaseName("idx_content_id_on_platform");

                    b.HasIndex("IsDeleted")
                        .HasDatabaseName("idx_content_is_deleted");

                    b.HasIndex("LastEditedById")
                        .HasDatabaseName("idx_content_last_edited_by_id");

                    b.HasIndex("MediaTypeId")
                        .HasDatabaseName("idx_content_media_type_id");

                    b.HasIndex("Priority")
                        .HasDatabaseName("idx_content_priority");

                    b.HasIndex("RatingId")
                        .HasDatabaseName("idx_content_rating_id");

                    b.HasIndex("ThumbnailId")
                        .HasDatabaseName("idx_content_thumbnail_id");

                    b.HasIndex("PlatformId", "IdOnPlatform")
                        .IsUnique()
                        .HasDatabaseName("uq_content_platform_id_and_id_on_platform");

                    b.ToTable("content");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentCreditableEntity", b =>
                {
                    b.Property<int>("ContentId")
                        .HasColumnType("integer")
                        .HasColumnName("content_id");

                    b.Property<int>("CreditableEntityId")
                        .HasColumnType("integer")
                        .HasColumnName("creditable_entity_id");

                    b.HasKey("ContentId", "CreditableEntityId")
                        .HasName("pk_content_creditable_entity");

                    b.HasIndex("CreditableEntityId")
                        .HasDatabaseName("idx_content_creditable_entity_creditable_entity_id");

                    b.ToTable("content_creditable_entity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentError", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("ContentId")
                        .HasColumnType("integer")
                        .HasColumnName("content_id");

                    b.Property<bool>("IsFatal")
                        .HasColumnType("boolean")
                        .HasColumnName("is_fatal");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.HasKey("Id")
                        .HasName("pk_content_error");

                    b.HasIndex("ContentId")
                        .HasDatabaseName("idx_content_error_content_id");

                    b.ToTable("content_error");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentFile", b =>
                {
                    b.Property<int>("ContentId")
                        .HasColumnType("integer")
                        .HasColumnName("content_id");

                    b.Property<string>("Location")
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("location");

                    b.Property<int>("FileFormatId")
                        .HasColumnType("integer")
                        .HasColumnName("file_format_id");

                    b.Property<int>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<int>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("ContentId", "Location")
                        .HasName("pk_content_file");

                    b.HasIndex("FileFormatId")
                        .HasDatabaseName("idx_content_file_file_format_id");

                    b.ToTable("content_file");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentRating", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<bool>("IsSfw")
                        .HasColumnType("boolean")
                        .HasColumnName("is_sfw");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_content_rating");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_content_rating_name");

                    b.ToTable("content_rating");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentWarning", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("ContentId")
                        .HasColumnType("integer")
                        .HasColumnName("content_id");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.HasKey("Id")
                        .HasName("pk_content_warning");

                    b.HasIndex("ContentId")
                        .HasDatabaseName("idx_content_warning_content_id");

                    b.ToTable("content_warning");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.CreditableEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<long?>("ChangeId")
                        .HasColumnType("bigint")
                        .HasColumnName("change_id");

                    b.Property<string>("IdOnPlatform")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("id_on_platform");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_creditable_entity");

                    b.HasIndex("ChangeId")
                        .IsUnique()
                        .HasDatabaseName("uq_creditable_entity_change_id");

                    b.HasIndex("IdOnPlatform")
                        .IsUnique()
                        .HasDatabaseName("uq_creditable_entity_id_on_platform");

                    b.HasIndex("PlatformId")
                        .HasDatabaseName("idx_creditable_entity_platform_id");

                    b.ToTable("creditable_entity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.FileFormat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("character varying(8)")
                        .HasColumnName("abbreviation");

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)")
                        .HasColumnName("extension");

                    b.Property<int?>("MediaTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("media_type_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_file_format");

                    b.HasIndex("Abbreviation")
                        .IsUnique()
                        .HasDatabaseName("uq_file_format_abbreviation");

                    b.HasIndex("Extension")
                        .IsUnique()
                        .HasDatabaseName("uq_file_format_extension");

                    b.HasIndex("MediaTypeId")
                        .HasDatabaseName("idx_file_format_media_type_id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_file_format_name");

                    b.ToTable("file_format");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ImageHash", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<byte[]>("PhashAverage1024")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_average1024");

                    b.Property<byte[]>("PhashAverage256")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_average256");

                    b.Property<byte[]>("PhashAverage64")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_average64");

                    b.Property<byte[]>("PhashBlue1024")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_blue1024");

                    b.Property<byte[]>("PhashBlue256")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_blue256");

                    b.Property<byte[]>("PhashBlue64")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_blue64");

                    b.Property<byte[]>("PhashGreen1024")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_green1024");

                    b.Property<byte[]>("PhashGreen256")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_green256");

                    b.Property<byte[]>("PhashGreen64")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_green64");

                    b.Property<byte[]>("PhashRed1024")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_red1024");

                    b.Property<byte[]>("PhashRed256")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_red256");

                    b.Property<byte[]>("PhashRed64")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_red64");

                    b.HasKey("Id")
                        .HasName("pk_image_hash");

                    b.ToTable("image_hash");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.IndexStatistic", b =>
                {
                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int>("MediaTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("media_type_id");

                    b.Property<int>("Count")
                        .HasColumnType("integer")
                        .HasColumnName("count");

                    b.Property<int>("IndexedCount")
                        .HasColumnType("integer")
                        .HasColumnName("indexed_count");

                    b.HasKey("PlatformId", "MediaTypeId")
                        .HasName("pk_index_statistic");

                    b.HasIndex("MediaTypeId")
                        .HasDatabaseName("idx_index_statistic_media_type_id");

                    b.ToTable("index_statistic");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.MediaType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_media_type");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_media_type_name");

                    b.ToTable("media_type");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_permission");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_permission_name");

                    b.ToTable("permission");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Platform", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<int>("EstimatedContentCount")
                        .HasColumnType("integer")
                        .HasColumnName("estimated_content_count");

                    b.Property<string>("HomeLocation")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("home_location");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("name");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("character varying(40)")
                        .HasColumnName("normalized_name");

                    b.HasKey("Id")
                        .HasName("pk_platform");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_platform_name");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("uq_platform_normalized_name");

                    b.ToTable("platform");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.PlatformSync", b =>
                {
                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int>("SyncTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("sync_type_id");

                    b.Property<TimeSpan>("Interval")
                        .HasColumnType("interval")
                        .HasColumnName("interval");

                    b.Property<DateTime>("When")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("when");

                    b.HasKey("PlatformId", "SyncTypeId")
                        .HasName("pk_platform_sync");

                    b.HasIndex("SyncTypeId")
                        .HasDatabaseName("idx_platform_sync_sync_type_id");

                    b.ToTable("platform_sync");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.SyncType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_sync_type");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_sync_type_name");

                    b.ToTable("sync_type");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Thumbnail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("CenterX")
                        .HasColumnType("integer")
                        .HasColumnName("center_x");

                    b.Property<int>("CenterY")
                        .HasColumnType("integer")
                        .HasColumnName("center_y");

                    b.Property<int>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("location");

                    b.Property<int>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("Id")
                        .HasName("pk_thumbnail");

                    b.ToTable("thumbnail");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Image", b =>
                {
                    b.HasBaseType("Noppes.Fluffle.Main.Database.Models.Content");

                    b.HasDiscriminator().HasValue("Image");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ApiKeyPermission", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.ApiKey", "ApiKey")
                        .WithMany("ApiKeyPermissions")
                        .HasForeignKey("ApiKeyId")
                        .HasConstraintName("fk_api_key")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Permission", "Permission")
                        .WithMany("ApiKeyPermissions")
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("fk_permissions")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ApiKey");

                    b.Navigation("Permission");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Content", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.ApiKey", "LastEditedBy")
                        .WithMany("LastEditedContent")
                        .HasForeignKey("LastEditedById")
                        .HasConstraintName("fk_api_key")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.MediaType", "MediaType")
                        .WithMany("Content")
                        .HasForeignKey("MediaTypeId")
                        .HasConstraintName("fk_media_types")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Platform", "Platform")
                        .WithMany("Content")
                        .HasForeignKey("PlatformId")
                        .HasConstraintName("fk_platforms")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.ContentRating", "Rating")
                        .WithMany("Content")
                        .HasForeignKey("RatingId")
                        .HasConstraintName("fk_image_ratings")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Thumbnail", "Thumbnail")
                        .WithMany("Content")
                        .HasForeignKey("ThumbnailId")
                        .HasConstraintName("fk_thumbnails")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("LastEditedBy");

                    b.Navigation("MediaType");

                    b.Navigation("Platform");

                    b.Navigation("Rating");

                    b.Navigation("Thumbnail");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentCreditableEntity", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Content", "Content")
                        .WithMany("ContentCreditableEntity")
                        .HasForeignKey("ContentId")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.CreditableEntity", "CreditableEntity")
                        .WithMany("ContentCreditableEntity")
                        .HasForeignKey("CreditableEntityId")
                        .HasConstraintName("fk_creditable_entities")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");

                    b.Navigation("CreditableEntity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentError", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Content", "Content")
                        .WithMany("Errors")
                        .HasForeignKey("ContentId")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentFile", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Content", "Content")
                        .WithMany("Files")
                        .HasForeignKey("ContentId")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.FileFormat", "Format")
                        .WithMany("ContentFiles")
                        .HasForeignKey("FileFormatId")
                        .HasConstraintName("fk_file_formats")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Content");

                    b.Navigation("Format");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentWarning", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Content", "Content")
                        .WithMany("Warnings")
                        .HasForeignKey("ContentId")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.CreditableEntity", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Platform", "Platform")
                        .WithMany("CreditableEntities")
                        .HasForeignKey("PlatformId")
                        .HasConstraintName("fk_platforms")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Platform");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.FileFormat", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.MediaType", null)
                        .WithMany("FileFormats")
                        .HasForeignKey("MediaTypeId")
                        .HasConstraintName("fk_media_types");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ImageHash", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Image", "Image")
                        .WithOne("ImageHash")
                        .HasForeignKey("Noppes.Fluffle.Main.Database.Models.ImageHash", "Id")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.IndexStatistic", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.MediaType", "MediaType")
                        .WithMany("IndexStatistics")
                        .HasForeignKey("MediaTypeId")
                        .HasConstraintName("fk_media_types")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Platform", "Platform")
                        .WithMany("IndexStatistics")
                        .HasForeignKey("PlatformId")
                        .HasConstraintName("fk_platforms")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaType");

                    b.Navigation("Platform");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.PlatformSync", b =>
                {
                    b.HasOne("Noppes.Fluffle.Main.Database.Models.Platform", "Platform")
                        .WithMany("PlatformSyncs")
                        .HasForeignKey("PlatformId")
                        .HasConstraintName("fk_platform")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Main.Database.Models.SyncType", "SyncType")
                        .WithMany("PlatformSyncs")
                        .HasForeignKey("SyncTypeId")
                        .HasConstraintName("fk_sync_types")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Platform");

                    b.Navigation("SyncType");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ApiKey", b =>
                {
                    b.Navigation("ApiKeyPermissions");

                    b.Navigation("LastEditedContent");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Content", b =>
                {
                    b.Navigation("ContentCreditableEntity");

                    b.Navigation("Errors");

                    b.Navigation("Files");

                    b.Navigation("Warnings");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.ContentRating", b =>
                {
                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.CreditableEntity", b =>
                {
                    b.Navigation("ContentCreditableEntity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.FileFormat", b =>
                {
                    b.Navigation("ContentFiles");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.MediaType", b =>
                {
                    b.Navigation("Content");

                    b.Navigation("FileFormats");

                    b.Navigation("IndexStatistics");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Permission", b =>
                {
                    b.Navigation("ApiKeyPermissions");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Platform", b =>
                {
                    b.Navigation("Content");

                    b.Navigation("CreditableEntities");

                    b.Navigation("IndexStatistics");

                    b.Navigation("PlatformSyncs");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.SyncType", b =>
                {
                    b.Navigation("PlatformSyncs");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Thumbnail", b =>
                {
                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Main.Database.Models.Image", b =>
                {
                    b.Navigation("ImageHash");
                });
#pragma warning restore 612, 618
        }
    }
}
