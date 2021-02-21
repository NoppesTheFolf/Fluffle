using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddIdOnPlatformAsIntegerToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_on_platform_as_integer",
                table: "content",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_content_id_on_platform_as_integer",
                table: "content",
                column: "id_on_platform_as_integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_id_on_platform_as_integer",
                table: "content");

            migrationBuilder.DropColumn(
                name: "id_on_platform_as_integer",
                table: "content");
        }
    }
}
