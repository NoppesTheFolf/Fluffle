using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class FillHasFatalErrorsColumnInContent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
UPDATE content
SET has_fatal_errors = TRUE
WHERE id IN (SELECT DISTINCT content_id FROM content_error WHERE is_fatal);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
