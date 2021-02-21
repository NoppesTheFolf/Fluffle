using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class FillIdOnPlatformAsInteger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE content
SET id_on_platform_as_integer = (
	CASE WHEN id_on_platform ~ '^[0-9]+$'
	THEN id_on_platform::int
	ELSE NULL END
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
