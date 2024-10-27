using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class RemoveSourceFromContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "content");

            migrationBuilder.DropColumn(
                name: "source_version",
                table: "content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "source",
                table: "content",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_version",
                table: "content",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
