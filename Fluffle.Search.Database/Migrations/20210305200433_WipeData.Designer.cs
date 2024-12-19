﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Noppes.Fluffle.Search.Database.Migrations
{
    [DbContext(typeof(FluffleSearchContext))]
    [Migration("20210305200433_WipeData")]
    partial class WipeData
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:Collation", "en_US.utf8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Content", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<long>("ChangeId")
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

                    b.Property<bool>("IsSfw")
                        .HasColumnType("boolean")
                        .HasColumnName("is_sfw");

                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int>("ThumbnailId")
                        .HasColumnType("integer")
                        .HasColumnName("thumbnail_id");

                    b.Property<string>("ViewLocation")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("view_location");

                    b.HasKey("Id")
                        .HasName("pk_content");

                    b.HasIndex("ChangeId")
                        .IsUnique()
                        .HasDatabaseName("uq_content_change_id");

                    b.HasIndex("IsSfw")
                        .HasDatabaseName("idx_content_is_sfw");

                    b.HasIndex("PlatformId")
                        .HasDatabaseName("idx_content_platform_id");

                    b.HasIndex("ThumbnailId")
                        .HasDatabaseName("idx_content_thumbnail_id");

                    b.ToTable("content");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ContentCreditableEntity", b =>
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

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ContentFile", b =>
                {
                    b.Property<int>("ContentId")
                        .HasColumnType("integer")
                        .HasColumnName("content_id");

                    b.Property<string>("Location")
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("location");

                    b.Property<int>("Format")
                        .HasColumnType("integer")
                        .HasColumnName("format");

                    b.Property<int>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<int>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("ContentId", "Location")
                        .HasName("pk_content_file");

                    b.ToTable("content_file");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.CreditableEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<long>("ChangeId")
                        .HasColumnType("bigint")
                        .HasColumnName("change_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_creditable_entity");

                    b.HasIndex("ChangeId")
                        .IsUnique()
                        .HasDatabaseName("uq_creditable_entity_change_id");

                    b.ToTable("creditable_entity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ImageHash", b =>
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

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Platform", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

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

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Thumbnail", b =>
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

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Image", b =>
                {
                    b.HasBaseType("Noppes.Fluffle.Search.Database.Models.Content");

                    b.HasDiscriminator().HasValue("Image");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Content", b =>
                {
                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Platform", "Platform")
                        .WithMany("Content")
                        .HasForeignKey("PlatformId")
                        .HasConstraintName("fk_platform")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Thumbnail", "Thumbnail")
                        .WithMany("Content")
                        .HasForeignKey("ThumbnailId")
                        .HasConstraintName("fk_thumbnails")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Platform");

                    b.Navigation("Thumbnail");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ContentCreditableEntity", b =>
                {
                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Content", "Content")
                        .WithMany("ContentCreditableEntities")
                        .HasForeignKey("ContentId")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Noppes.Fluffle.Search.Database.Models.CreditableEntity", "CreditableEntity")
                        .WithMany("ContentCreditableEntity")
                        .HasForeignKey("CreditableEntityId")
                        .HasConstraintName("fk_creditable_entities")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");

                    b.Navigation("CreditableEntity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ContentFile", b =>
                {
                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Content", "Content")
                        .WithMany("Files")
                        .HasForeignKey("ContentId")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ImageHash", b =>
                {
                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Image", "Image")
                        .WithOne("ImageHash")
                        .HasForeignKey("Noppes.Fluffle.Search.Database.Models.ImageHash", "Id")
                        .HasConstraintName("fk_content")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Content", b =>
                {
                    b.Navigation("ContentCreditableEntities");

                    b.Navigation("Files");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.CreditableEntity", b =>
                {
                    b.Navigation("ContentCreditableEntity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Platform", b =>
                {
                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Thumbnail", b =>
                {
                    b.Navigation("Content");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Image", b =>
                {
                    b.Navigation("ImageHash");
                });
#pragma warning restore 612, 618
        }
    }
}
