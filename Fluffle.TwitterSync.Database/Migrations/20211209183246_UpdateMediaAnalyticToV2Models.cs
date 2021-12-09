using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations
{
    public partial class UpdateMediaAnalyticToV2Models : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "anime",
                table: "media_analytic");

            migrationBuilder.DropColumn(
                name: "artist_ids",
                table: "media_analytic");

            migrationBuilder.DropColumn(
                name: "furry_art",
                table: "media_analytic");

            migrationBuilder.RenameColumn(
                name: "real",
                table: "media_analytic",
                newName: "true");

            migrationBuilder.RenameColumn(
                name: "fursuit",
                table: "media_analytic",
                newName: "false");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "true",
                table: "media_analytic",
                newName: "real");

            migrationBuilder.RenameColumn(
                name: "false",
                table: "media_analytic",
                newName: "fursuit");

            migrationBuilder.AddColumn<double>(
                name: "anime",
                table: "media_analytic",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int[]>(
                name: "artist_ids",
                table: "media_analytic",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<double>(
                name: "furry_art",
                table: "media_analytic",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
