﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Noppes.Fluffle.Search.Database.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    [DbContext(typeof(FluffleSearchContext))]
    partial class FluffleSearchContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("en_US.utf8")
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ApiKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("character varying")
                        .HasColumnName("description");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character(32)")
                        .HasColumnName("key")
                        .IsFixedLength();

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_api_key");

                    b.HasIndex("Key")
                        .IsUnique()
                        .HasDatabaseName("uq_api_key_key");

                    b.ToTable("api_key");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ApiKeyPermission", b =>
                {
                    b.Property<int>("ApiKeyId")
                        .HasColumnType("integer")
                        .HasColumnName("api_key_id");

                    b.Property<int>("PermissionId")
                        .HasColumnType("integer")
                        .HasColumnName("permission_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("ApiKeyId", "PermissionId")
                        .HasName("pk_api_key_permission");

                    b.HasIndex("PermissionId")
                        .HasDatabaseName("idx_api_key_permission_permission_id");

                    b.ToTable("api_key_permission");
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

                    b.HasIndex("ContentId")
                        .HasDatabaseName("idx_content_file_content_id");

                    b.ToTable("content_file");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.CreditableEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("ChangeId")
                        .HasColumnType("bigint")
                        .HasColumnName("change_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int?>("Priority")
                        .HasColumnType("integer")
                        .HasColumnName("priority");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_creditable_entity");

                    b.HasIndex("PlatformId", "ChangeId")
                        .HasDatabaseName("idx_creditable_entity_platform_id_and_change_id");

                    b.ToTable("creditable_entity");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.DenormalizedImage", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<long>("ChangeId")
                        .HasColumnType("bigint")
                        .HasColumnName("change_id");

                    b.Property<int[]>("Credits")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasColumnName("credits");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool>("IsSfw")
                        .HasColumnType("boolean")
                        .HasColumnName("is_sfw");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("location");

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

                    b.Property<byte[]>("PhashGreen1024")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_green1024");

                    b.Property<byte[]>("PhashGreen256")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_green256");

                    b.Property<byte[]>("PhashRed1024")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_red1024");

                    b.Property<byte[]>("PhashRed256")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("phash_red256");

                    b.Property<int>("PlatformId")
                        .HasColumnType("integer")
                        .HasColumnName("platform_id");

                    b.Property<int>("ThumbnailCenterX")
                        .HasColumnType("integer")
                        .HasColumnName("thumbnail_center_x");

                    b.Property<int>("ThumbnailCenterY")
                        .HasColumnType("integer")
                        .HasColumnName("thumbnail_center_y");

                    b.Property<int>("ThumbnailHeight")
                        .HasColumnType("integer")
                        .HasColumnName("thumbnail_height");

                    b.Property<string>("ThumbnailLocation")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("thumbnail_location");

                    b.Property<int>("ThumbnailWidth")
                        .HasColumnType("integer")
                        .HasColumnName("thumbnail_width");

                    b.HasKey("Id")
                        .HasName("pk_denormalized_image");

                    b.HasIndex("PlatformId", "ChangeId")
                        .IsUnique()
                        .HasDatabaseName("uq_denormalized_image_platform_id_and_change_id");

                    b.ToTable("denormalized_image");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("name");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_permission");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("uq_permission_name");

                    b.ToTable("permission");
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

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.SearchRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AreaCheck")
                        .HasColumnType("integer")
                        .HasColumnName("area_check");

                    b.Property<int?>("Compare64Average")
                        .HasColumnType("integer")
                        .HasColumnName("compare64_average");

                    b.Property<int?>("ComplementComparisonResults")
                        .HasColumnType("integer")
                        .HasColumnName("complement_comparison_results");

                    b.Property<int?>("Compute64Average")
                        .HasColumnType("integer")
                        .HasColumnName("compute64_average");

                    b.Property<int?>("ComputeExpensiveBlue")
                        .HasColumnType("integer")
                        .HasColumnName("compute_expensive_blue");

                    b.Property<int?>("ComputeExpensiveGreen")
                        .HasColumnType("integer")
                        .HasColumnName("compute_expensive_green");

                    b.Property<int?>("ComputeExpensiveRed")
                        .HasColumnType("integer")
                        .HasColumnName("compute_expensive_red");

                    b.Property<int?>("Count")
                        .HasColumnType("integer")
                        .HasColumnName("count");

                    b.Property<int?>("CreateAndRefineOutput")
                        .HasColumnType("integer")
                        .HasColumnName("create_and_refine_output");

                    b.Property<string>("Exception")
                        .HasColumnType("text")
                        .HasColumnName("exception");

                    b.Property<int?>("Flush")
                        .HasColumnType("integer")
                        .HasColumnName("flush");

                    b.Property<int?>("Format")
                        .HasColumnType("integer")
                        .HasColumnName("format");

                    b.Property<string>("From")
                        .HasColumnType("text")
                        .HasColumnName("from");

                    b.Property<int?>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<bool?>("LinkCreated")
                        .HasColumnType("boolean")
                        .HasColumnName("link_created");

                    b.Property<string>("QueryId")
                        .HasColumnType("text")
                        .HasColumnName("query_id");

                    b.Property<int>("Sequence")
                        .HasColumnType("integer")
                        .HasColumnName("sequence");

                    b.Property<int?>("StartExpensiveRgbComputation")
                        .HasColumnType("integer")
                        .HasColumnName("start_expensive_rgb_computation");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("started_at");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_agent");

                    b.Property<int?>("WaitForExpensiveRgbComputation")
                        .HasColumnType("integer")
                        .HasColumnName("wait_for_expensive_rgb_computation");

                    b.Property<int?>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("Id")
                        .HasName("pk_search_request");

                    b.HasIndex("LinkCreated")
                        .HasDatabaseName("idx_search_request_link_created");

                    b.HasIndex("QueryId")
                        .IsUnique()
                        .HasDatabaseName("uq_search_request_query_id");

                    b.ToTable("search_request");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.SearchRequestV2", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<int?>("AlternativeCount")
                        .HasColumnType("integer")
                        .HasColumnName("alternative_count");

                    b.Property<int?>("AppendCreditableEntities")
                        .HasColumnType("integer")
                        .HasColumnName("append_creditable_entities");

                    b.Property<int?>("AreaCheck")
                        .HasColumnType("integer")
                        .HasColumnName("area_check");

                    b.Property<int?>("CleanViewLocation")
                        .HasColumnType("integer")
                        .HasColumnName("clean_view_location");

                    b.Property<int?>("CompareCoarse")
                        .HasColumnType("integer")
                        .HasColumnName("compare_coarse");

                    b.Property<int?>("CompareGranular")
                        .HasColumnType("integer")
                        .HasColumnName("compare_granular");

                    b.Property<int?>("Compute1024Average")
                        .HasColumnType("integer")
                        .HasColumnName("compute1024_average");

                    b.Property<int?>("Compute1024Blue")
                        .HasColumnType("integer")
                        .HasColumnName("compute1024_blue");

                    b.Property<int?>("Compute1024Green")
                        .HasColumnType("integer")
                        .HasColumnName("compute1024_green");

                    b.Property<int?>("Compute1024Red")
                        .HasColumnType("integer")
                        .HasColumnName("compute1024_red");

                    b.Property<int?>("Compute256Average")
                        .HasColumnType("integer")
                        .HasColumnName("compute256_average");

                    b.Property<int?>("Compute256Blue")
                        .HasColumnType("integer")
                        .HasColumnName("compute256_blue");

                    b.Property<int?>("Compute256Green")
                        .HasColumnType("integer")
                        .HasColumnName("compute256_green");

                    b.Property<int?>("Compute256Red")
                        .HasColumnType("integer")
                        .HasColumnName("compute256_red");

                    b.Property<int?>("Compute64Average")
                        .HasColumnType("integer")
                        .HasColumnName("compute64_average");

                    b.Property<int?>("Count")
                        .HasColumnType("integer")
                        .HasColumnName("count");

                    b.Property<int?>("DetermineFinalOrder")
                        .HasColumnType("integer")
                        .HasColumnName("determine_final_order");

                    b.Property<int?>("EnqueueLinkCreation")
                        .HasColumnType("integer")
                        .HasColumnName("enqueue_link_creation");

                    b.Property<int?>("ExactCount")
                        .HasColumnType("integer")
                        .HasColumnName("exact_count");

                    b.Property<string>("Exception")
                        .HasColumnType("text")
                        .HasColumnName("exception");

                    b.Property<int?>("Finish")
                        .HasColumnType("integer")
                        .HasColumnName("finish");

                    b.Property<int?>("Flush")
                        .HasColumnType("integer")
                        .HasColumnName("flush");

                    b.Property<int?>("Format")
                        .HasColumnType("integer")
                        .HasColumnName("format");

                    b.Property<string>("From")
                        .HasColumnType("text")
                        .HasColumnName("from");

                    b.Property<int?>("Height")
                        .HasColumnType("integer")
                        .HasColumnName("height");

                    b.Property<bool?>("LinkCreated")
                        .HasColumnType("boolean")
                        .HasColumnName("link_created");

                    b.Property<int?>("LinkCreationPreparation")
                        .HasColumnType("integer")
                        .HasColumnName("link_creation_preparation");

                    b.Property<int?>("ReduceCoarseResults")
                        .HasColumnType("integer")
                        .HasColumnName("reduce_coarse_results");

                    b.Property<int?>("ReduceGranularResults")
                        .HasColumnType("integer")
                        .HasColumnName("reduce_granular_results");

                    b.Property<int?>("RetrieveCreditableEntities")
                        .HasColumnType("integer")
                        .HasColumnName("retrieve_creditable_entities");

                    b.Property<int?>("RetrieveImageInfo")
                        .HasColumnType("integer")
                        .HasColumnName("retrieve_image_info");

                    b.Property<int>("Sequence")
                        .HasColumnType("integer")
                        .HasColumnName("sequence");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("started_at");

                    b.Property<int?>("TossUpCount")
                        .HasColumnType("integer")
                        .HasColumnName("toss_up_count");

                    b.Property<int?>("UnlikelyCount")
                        .HasColumnType("integer")
                        .HasColumnName("unlikely_count");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_agent");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version");

                    b.Property<int?>("Width")
                        .HasColumnType("integer")
                        .HasColumnName("width");

                    b.HasKey("Id")
                        .HasName("pk_search_request_v2");

                    b.HasIndex("LinkCreated")
                        .HasDatabaseName("idx_search_request_v2_link_created");

                    b.ToTable("search_request_v2");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ApiKeyPermission", b =>
                {
                    b.HasOne("Noppes.Fluffle.Search.Database.Models.ApiKey", "ApiKey")
                        .WithMany("ApiKeyPermissions")
                        .HasForeignKey("ApiKeyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_api_key");

                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Permission", "Permission")
                        .WithMany("ApiKeyPermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_permissions");

                    b.Navigation("ApiKey");

                    b.Navigation("Permission");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.CreditableEntity", b =>
                {
                    b.HasOne("Noppes.Fluffle.Search.Database.Models.Platform", "Platform")
                        .WithMany("CreditableEntities")
                        .HasForeignKey("PlatformId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_platform");

                    b.Navigation("Platform");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.ApiKey", b =>
                {
                    b.Navigation("ApiKeyPermissions");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Permission", b =>
                {
                    b.Navigation("ApiKeyPermissions");
                });

            modelBuilder.Entity("Noppes.Fluffle.Search.Database.Models.Platform", b =>
                {
                    b.Navigation("CreditableEntities");
                });
#pragma warning restore 612, 618
        }
    }
}
