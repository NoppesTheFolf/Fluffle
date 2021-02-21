using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class DeriveThumbnailFilename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE thumbnail
SET filename = substring(location, '^https:\/\/.+?\/.+?\/.+?\/(.+)$');
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
