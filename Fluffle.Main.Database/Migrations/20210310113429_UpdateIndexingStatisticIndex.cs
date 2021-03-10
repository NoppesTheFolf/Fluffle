using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class UpdateIndexingStatisticIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_platform_id_and_media_type_id_and_is_indexed",
                table: "content");

            migrationBuilder.CreateIndex(
                name: "idx_content_is_deleted_and_platform_id_and_media_type_id_and_is_indexed",
                table: "content",
                columns: new[] { "is_deleted", "platform_id", "media_type_id", "is_indexed" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_is_deleted_and_platform_id_and_media_type_id_and_is_indexed",
                table: "content");

            migrationBuilder.CreateIndex(
                name: "idx_content_platform_id_and_media_type_id_and_is_indexed",
                table: "content",
                columns: new[] { "platform_id", "media_type_id", "is_indexed" });
        }
    }
}
