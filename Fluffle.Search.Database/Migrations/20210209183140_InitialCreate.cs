using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "creditable_entity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    change_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_creditable_entity", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "platform",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform", x => x.id);
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
                name: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    change_id = table.Column<long>(type: "bigint", nullable: false),
                    view_location = table.Column<string>(type: "text", nullable: false),
                    is_sfw = table.Column<bool>(type: "boolean", nullable: false),
                    thumbnail_id = table.Column<int>(type: "integer", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    phash_red64 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_green64 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_blue64 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_average64 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_red256 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_green256 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_blue256 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_average256 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_red1024 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_green1024 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_blue1024 = table.Column<byte[]>(type: "bytea", nullable: true),
                    phash_average1024 = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content", x => x.id);
                    table.ForeignKey(
                        name: "fk_platform",
                        column: x => x.platform_id,
                        principalTable: "platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_thumbnails",
                        column: x => x.thumbnail_id,
                        principalTable: "thumbnail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "idx_content_is_sfw",
                table: "content",
                column: "is_sfw");

            migrationBuilder.CreateIndex(
                name: "idx_content_platform_id",
                table: "content",
                column: "platform_id");

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
                name: "idx_content_creditable_entity_creditable_entity_id",
                table: "content_creditable_entity",
                column: "creditable_entity_id");

            migrationBuilder.CreateIndex(
                name: "uq_creditable_entity_change_id",
                table: "creditable_entity",
                column: "change_id",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_creditable_entity");

            migrationBuilder.DropTable(
                name: "content");

            migrationBuilder.DropTable(
                name: "creditable_entity");

            migrationBuilder.DropTable(
                name: "platform");

            migrationBuilder.DropTable(
                name: "thumbnail");
        }
    }
}
