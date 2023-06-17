using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class RemoveThumbnail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_thumbnails",
                table: "content");

            migrationBuilder.DropTable(
                name: "thumbnail");

            migrationBuilder.DropIndex(
                name: "idx_content_thumbnail_id",
                table: "content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "thumbnail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    center_x = table.Column<int>(type: "integer", nullable: false),
                    center_y = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thumbnail", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_content_thumbnail_id",
                table: "content",
                column: "thumbnail_id");

            migrationBuilder.AddForeignKey(
                name: "fk_thumbnails",
                table: "content",
                column: "thumbnail_id",
                principalTable: "thumbnail",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
