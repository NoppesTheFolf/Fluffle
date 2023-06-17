using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class AddIndexForContentFileOnContentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_content_file_content_id",
                table: "content_file",
                column: "content_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_file_content_id",
                table: "content_file");
        }
    }
}
