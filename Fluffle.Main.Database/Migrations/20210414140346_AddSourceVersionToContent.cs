using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddSourceVersionToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "source_version",
                table: "content",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_version",
                table: "content");
        }
    }
}
