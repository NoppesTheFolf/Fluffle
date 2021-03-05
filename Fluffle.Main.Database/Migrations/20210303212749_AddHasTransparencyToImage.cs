using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddHasTransparencyToImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_transparency",
                table: "content",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_transparency",
                table: "content");
        }
    }
}
