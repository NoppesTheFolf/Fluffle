using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_content_discriminator_and_change_id",
                table: "content",
                columns: new[] { "discriminator", "change_id" });

            migrationBuilder.CreateIndex(
                name: "idx_content_is_deleted_and_is_marked_for_deletion",
                table: "content",
                columns: new[] { "is_deleted", "is_marked_for_deletion" });

            migrationBuilder.CreateIndex(
                name: "idx_content_platform_id_and_media_type_id_and_is_indexed",
                table: "content",
                columns: new[] { "platform_id", "media_type_id", "is_indexed" });

            migrationBuilder.Sql(@"
CREATE INDEX idx_content_unprocessed ON content (
    discriminator,
    is_marked_for_deletion DESC,
    is_deleted DESC,
    platform_id,
    media_type_id,
    requires_indexing,
    is_indexed,
    priority DESC,
    id DESC
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_discriminator_and_change_id",
                table: "content");

            migrationBuilder.DropIndex(
                name: "idx_content_is_deleted_and_is_marked_for_deletion",
                table: "content");

            migrationBuilder.DropIndex(
                name: "idx_content_platform_id_and_media_type_id_and_is_indexed",
                table: "content");

            migrationBuilder.DropIndex(
                name: "idx_content_unprocessed",
                table: "content");
        }
    }
}
