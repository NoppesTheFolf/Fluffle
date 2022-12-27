using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddWildcardIndexForContentIdOnPlatform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE INDEX idx_content_platform_id_and_id_on_platform_wildcard ON content (platform_id, id_on_platform text_pattern_ops);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX idx_content_platform_id_and_id_on_platform_wildcard;");
        }
    }
}
