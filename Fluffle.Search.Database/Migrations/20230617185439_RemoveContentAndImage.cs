using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class RemoveContentAndImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    change_id = table.Column<long>(type: "bigint", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    id_on_platform = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_sfw = table.Column<bool>(type: "boolean", nullable: false),
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_id = table.Column<int>(type: "integer", nullable: false),
                    view_location = table.Column<string>(type: "text", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "idx_content_is_sfw",
                table: "content",
                column: "is_sfw");

            migrationBuilder.CreateIndex(
                name: "uq_content_platform_id_and_change_id",
                table: "content",
                columns: new[] { "platform_id", "change_id" },
                unique: true);
        }
    }
}
