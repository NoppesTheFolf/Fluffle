using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddReservedUntilToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "reserved_until",
                table: "content",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // This index has become obsolete because the way unprocessed content is retrieved will
            // be changed
            migrationBuilder.DropIndex(
                name: "idx_content_unprocessed",
                table: "content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reserved_until",
                table: "content");

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
    }
}
