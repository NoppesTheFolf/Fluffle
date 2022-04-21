using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddRetryColumnsToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_fatal_errors",
                table: "content",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "retry_increment",
                table: "content",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "retry_reserved_until",
                table: "content",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_fatal_errors",
                table: "content");

            migrationBuilder.DropColumn(
                name: "retry_increment",
                table: "content");

            migrationBuilder.DropColumn(
                name: "retry_reserved_until",
                table: "content");
        }
    }
}
