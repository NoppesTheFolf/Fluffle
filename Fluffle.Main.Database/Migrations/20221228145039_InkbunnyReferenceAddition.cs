using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class InkbunnyReferenceAddition : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE content SET reference = SPLIT_PART(id_on_platform, '-', 1) WHERE platform_id = 7;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE content SET reference = NULL WHERE platform_id = 7;");
    }
}
