using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class RemoveUnusedContentFileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_file");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_file",
                columns: table => new
                {
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    format = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_file", x => new { x.content_id, x.location });
                });

            migrationBuilder.CreateIndex(
                name: "idx_content_file_content_id",
                table: "content_file",
                column: "content_id");
        }
    }
}
