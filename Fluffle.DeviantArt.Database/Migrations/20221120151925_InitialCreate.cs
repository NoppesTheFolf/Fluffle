using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.DeviantArt.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deviant",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    icon_location = table.Column<string>(type: "text", nullable: false),
                    joined_when = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_furry_artist = table.Column<bool>(type: "boolean", nullable: true),
                    is_furry_artist_enqueued_when = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_furry_artist_determined_when = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gallery_scraped_when = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deviant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "deviation",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    deviant_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deviation", x => x.id);
                    table.ForeignKey(
                        name: "fk_deviant",
                        column: x => x.deviant_id,
                        principalTable: "deviant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_deviant_is_furry_artist",
                table: "deviant",
                column: "is_furry_artist");

            migrationBuilder.CreateIndex(
                name: "idx_deviation_deviant_id",
                table: "deviation",
                column: "deviant_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deviation");

            migrationBuilder.DropTable(
                name: "deviant");
        }
    }
}
