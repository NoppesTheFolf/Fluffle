using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class AddIndexToSpeedUpRetrievalOfContentToRetry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE INDEX idx_id_and_has_fatal_errors_and_retry_increment_and_retry_reserved_until_and_platform_id ON content (
    id DESC,
    has_fatal_errors,
    retry_increment,
    retry_reserved_until,
    platform_id
);
");

        migrationBuilder.Sql("UPDATE content SET retry_increment = 0, retry_reserved_until = 0;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
