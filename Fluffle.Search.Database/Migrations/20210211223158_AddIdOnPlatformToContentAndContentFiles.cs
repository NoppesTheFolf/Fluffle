using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class AddIdOnPlatformToContentAndContentFiles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "id_on_platform",
            table: "content",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "content_file",
            columns: table => new
            {
                content_id = table.Column<int>(type: "integer", nullable: false),
                location = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                format = table.Column<int>(type: "integer", nullable: false),
                width = table.Column<int>(type: "integer", nullable: false),
                height = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_content_file", x => new { x.content_id, x.location });
                table.ForeignKey(
                    name: "fk_content",
                    column: x => x.content_id,
                    principalTable: "content",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_content",
            table: "content_creditable_entity");

        migrationBuilder.DropTable(
            name: "content_file");

        migrationBuilder.DropColumn(
            name: "id_on_platform",
            table: "content");
    }
}
