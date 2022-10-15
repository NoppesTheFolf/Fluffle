using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class ChangeUnprocessedContentIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX idx_content_unprocessed;");

            migrationBuilder.Sql(@"
CREATE INDEX idx_content_unprocessed ON content (
    discriminator,
    is_marked_for_deletion DESC,
    is_deleted DESC,
    platform_id,
    requires_indexing,
    is_indexed,
    priority DESC,
    created_at DESC
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX idx_content_unprocessed;");

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
    created_at DESC
);
");
        }
    }
}
