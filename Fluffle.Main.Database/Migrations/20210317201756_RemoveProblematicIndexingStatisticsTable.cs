using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class RemoveProblematicIndexingStatisticsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_platforms",
            table: "index_statistic");

        migrationBuilder.DropForeignKey(
            name: "fk_media_types",
            table: "index_statistic");

        migrationBuilder.DropTable(
            name: "index_statistic");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "index_statistic",
            columns: table => new
            {
                platform_id = table.Column<int>(type: "integer", nullable: false),
                media_type_id = table.Column<int>(type: "integer", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                indexed_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_index_statistic", x => new { x.platform_id, x.media_type_id });
                table.ForeignKey(
                    name: "fk_media_types",
                    column: x => x.media_type_id,
                    principalTable: "media_type",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_platforms",
                    column: x => x.platform_id,
                    principalTable: "platform",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "idx_index_statistic_media_type_id",
            table: "index_statistic",
            column: "media_type_id");
    }
}
