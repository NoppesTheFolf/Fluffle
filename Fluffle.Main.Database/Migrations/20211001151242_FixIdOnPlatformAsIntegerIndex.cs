using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class FixIdOnPlatformAsIntegerIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_id_on_platform_as_integer",
                table: "content");

            migrationBuilder.CreateIndex(
                name: "idx_content_id_on_platform_as_integer_and_platform_id",
                table: "content",
                columns: new[] { "id_on_platform_as_integer", "platform_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_id_on_platform_as_integer_and_platform_id",
                table: "content");

            migrationBuilder.CreateIndex(
                name: "idx_content_id_on_platform_as_integer",
                table: "content",
                column: "id_on_platform_as_integer");
        }
    }
}
