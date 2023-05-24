using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class AddSyncStateToPlatform : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "sync_state",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                document = table.Column<string>(type: "text", nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sync_state", x => x.id);
                table.ForeignKey(
                    name: "fk_platforms",
                    column: x => x.id,
                    principalTable: "platform",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "sync_state");
    }
}
