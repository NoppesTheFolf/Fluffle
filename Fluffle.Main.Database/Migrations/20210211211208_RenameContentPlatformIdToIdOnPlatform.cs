using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class RenameContentPlatformIdToIdOnPlatform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "platform_content_id",
                table: "content",
                newName: "id_on_platform");

            migrationBuilder.RenameIndex(
                name: "uq_content_platform_id_and_platform_content_id",
                table: "content",
                newName: "uq_content_platform_id_and_id_on_platform");

            migrationBuilder.RenameIndex(
                name: "idx_content_platform_content_id",
                table: "content",
                newName: "idx_content_id_on_platform");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id_on_platform",
                table: "content",
                newName: "platform_content_id");

            migrationBuilder.RenameIndex(
                name: "uq_content_platform_id_and_id_on_platform",
                table: "content",
                newName: "uq_content_platform_id_and_platform_content_id");

            migrationBuilder.RenameIndex(
                name: "idx_content_id_on_platform",
                table: "content",
                newName: "idx_content_platform_content_id");
        }
    }
}
