using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations;

public partial class AddOtherSource : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "other_source",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                location = table.Column<string>(type: "text", nullable: false),
                has_been_processed = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_other_source", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "idx_other_source_has_been_processed",
            table: "other_source",
            column: "has_been_processed");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "other_source");
    }
}
