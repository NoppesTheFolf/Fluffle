using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddSourceToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "source",
                table: "content",
                type: "bytea",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "content");
        }
    }
}
