using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class Rename256RgbToExpensiveRgb : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "wait_for256_rgb_computation",
            table: "search_request",
            newName: "wait_for_expensive_rgb_computation");

        migrationBuilder.RenameColumn(
            name: "start256_rgb_computation",
            table: "search_request",
            newName: "start_expensive_rgb_computation");

        migrationBuilder.RenameColumn(
            name: "compute256_red",
            table: "search_request",
            newName: "compute_expensive_red");

        migrationBuilder.RenameColumn(
            name: "compute256_green",
            table: "search_request",
            newName: "compute_expensive_green");

        migrationBuilder.RenameColumn(
            name: "compute256_blue",
            table: "search_request",
            newName: "compute_expensive_blue");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "wait_for_expensive_rgb_computation",
            table: "search_request",
            newName: "wait_for256_rgb_computation");

        migrationBuilder.RenameColumn(
            name: "start_expensive_rgb_computation",
            table: "search_request",
            newName: "start256_rgb_computation");

        migrationBuilder.RenameColumn(
            name: "compute_expensive_red",
            table: "search_request",
            newName: "compute256_red");

        migrationBuilder.RenameColumn(
            name: "compute_expensive_green",
            table: "search_request",
            newName: "compute256_green");

        migrationBuilder.RenameColumn(
            name: "compute_expensive_blue",
            table: "search_request",
            newName: "compute256_blue");
    }
}
