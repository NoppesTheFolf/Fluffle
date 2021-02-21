using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_key",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character(32)", fixedLength: true, maxLength: 32, nullable: false),
                    description = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_rating",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_sfw = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_rating", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "platform",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    estimated_content_count = table.Column<int>(type: "integer", nullable: false),
                    home_location = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sync_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sync_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "thumbnail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    center_x = table.Column<int>(type: "integer", nullable: false),
                    center_y = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thumbnail", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_format",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    abbreviation = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    extension = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_format", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_types",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "api_key_permission",
                columns: table => new
                {
                    api_key_id = table.Column<int>(type: "integer", nullable: false),
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key_permission", x => new { x.api_key_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_api_key",
                        column: x => x.api_key_id,
                        principalTable: "api_key",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_permissions",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "creditable_entity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_on_platform = table.Column<string>(type: "text", nullable: false),
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    change_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_creditable_entity", x => x.id);
                    table.ForeignKey(
                        name: "fk_platforms",
                        column: x => x.platform_id,
                        principalTable: "platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "index_statistic",
                columns: table => new
                {
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    indexed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_index_statistic", x => new { x.platform_id, x.media_type_id });
                    table.ForeignKey(
                        name: "fk_media_types",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_platforms",
                        column: x => x.platform_id,
                        principalTable: "platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_sync",
                columns: table => new
                {
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    sync_type_id = table.Column<int>(type: "integer", nullable: false),
                    interval = table.Column<TimeSpan>(type: "interval", nullable: false),
                    when = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_sync", x => new { x.platform_id, x.sync_type_id });
                    table.ForeignKey(
                        name: "fk_platform",
                        column: x => x.platform_id,
                        principalTable: "platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sync_types",
                        column: x => x.sync_type_id,
                        principalTable: "sync_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    platform_content_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    view_location = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    rating_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    change_id = table.Column<long>(type: "bigint", nullable: true),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    last_edited_by_id = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_id = table.Column<int>(type: "integer", nullable: true),
                    requires_indexing = table.Column<bool>(type: "boolean", nullable: false),
                    is_indexed = table.Column<bool>(type: "boolean", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_key",
                        column: x => x.last_edited_by_id,
                        principalTable: "api_key",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_image_ratings",
                        column: x => x.rating_id,
                        principalTable: "content_rating",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_media_types",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_platforms",
                        column: x => x.platform_id,
                        principalTable: "platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_thumbnails",
                        column: x => x.thumbnail_id,
                        principalTable: "thumbnail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_creditable_entity",
                columns: table => new
                {
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    creditable_entity_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_creditable_entity", x => new { x.content_id, x.creditable_entity_id });
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_creditable_entities",
                        column: x => x.creditable_entity_id,
                        principalTable: "creditable_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_error",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message = table.Column<string>(type: "text", nullable: false),
                    is_fatal = table.Column<bool>(type: "boolean", nullable: false),
                    content_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_error", x => x.id);
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_file",
                columns: table => new
                {
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    file_format_id = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_file", x => new { x.content_id, x.location });
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_file_formats",
                        column: x => x.file_format_id,
                        principalTable: "file_format",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_warning",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_warning", x => x.id);
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "image_hash",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    phash_red64 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_green64 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_blue64 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_average64 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_red256 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_green256 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_blue256 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_average256 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_red1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_green1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_blue1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                    phash_average1024 = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_image_hash", x => x.id);
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_api_key_key",
                table: "api_key",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_api_key_permission_permission_id",
                table: "api_key_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_is_deleted",
                table: "content",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "idx_content_last_edited_by_id",
                table: "content",
                column: "last_edited_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_media_type_id",
                table: "content",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_platform_content_id",
                table: "content",
                column: "platform_content_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_priority",
                table: "content",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "idx_content_rating_id",
                table: "content",
                column: "rating_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_thumbnail_id",
                table: "content",
                column: "thumbnail_id");

            migrationBuilder.CreateIndex(
                name: "uq_content_change_id",
                table: "content",
                column: "change_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_content_platform_id_and_platform_content_id",
                table: "content",
                columns: new[] { "platform_id", "platform_content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_content_creditable_entity_creditable_entity_id",
                table: "content_creditable_entity",
                column: "creditable_entity_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_error_content_id",
                table: "content_error",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_file_file_format_id",
                table: "content_file",
                column: "file_format_id");

            migrationBuilder.CreateIndex(
                name: "uq_content_rating_name",
                table: "content_rating",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_content_warning_content_id",
                table: "content_warning",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "idx_creditable_entity_platform_id",
                table: "creditable_entity",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "uq_creditable_entity_change_id",
                table: "creditable_entity",
                column: "change_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_creditable_entity_id_on_platform",
                table: "creditable_entity",
                column: "id_on_platform",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_file_format_media_type_id",
                table: "file_format",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "uq_file_format_abbreviation",
                table: "file_format",
                column: "abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_file_format_extension",
                table: "file_format",
                column: "extension",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_file_format_name",
                table: "file_format",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_index_statistic_media_type_id",
                table: "index_statistic",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "uq_media_type_name",
                table: "media_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_permission_name",
                table: "permission",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_platform_name",
                table: "platform",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_platform_normalized_name",
                table: "platform",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_platform_sync_sync_type_id",
                table: "platform_sync",
                column: "sync_type_id");

            migrationBuilder.CreateIndex(
                name: "uq_sync_type_name",
                table: "sync_type",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_key_permission");

            migrationBuilder.DropTable(
                name: "content_creditable_entity");

            migrationBuilder.DropTable(
                name: "content_error");

            migrationBuilder.DropTable(
                name: "content_file");

            migrationBuilder.DropTable(
                name: "content_warning");

            migrationBuilder.DropTable(
                name: "image_hash");

            migrationBuilder.DropTable(
                name: "index_statistic");

            migrationBuilder.DropTable(
                name: "platform_sync");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "creditable_entity");

            migrationBuilder.DropTable(
                name: "file_format");

            migrationBuilder.DropTable(
                name: "content");

            migrationBuilder.DropTable(
                name: "sync_type");

            migrationBuilder.DropTable(
                name: "api_key");

            migrationBuilder.DropTable(
                name: "content_rating");

            migrationBuilder.DropTable(
                name: "media_type");

            migrationBuilder.DropTable(
                name: "platform");

            migrationBuilder.DropTable(
                name: "thumbnail");
        }
    }
}
