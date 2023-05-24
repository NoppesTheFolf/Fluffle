using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class AddReferenceColumnToContent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "reference",
            table: "content",
            type: "text",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "idx_content_platform_id_and_reference",
            table: "content",
            columns: new[] { "platform_id", "reference" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_content_platform_id_and_reference",
            table: "content");

        migrationBuilder.DropColumn(
            name: "reference",
            table: "content");
    }
}
