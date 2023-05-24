using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class AddResultMatchCountsToSearchRequest : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "alternative_count",
            table: "search_request_v2",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "exact_count",
            table: "search_request_v2",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "toss_up_count",
            table: "search_request_v2",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "unlikely_count",
            table: "search_request_v2",
            type: "integer",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "alternative_count",
            table: "search_request_v2");

        migrationBuilder.DropColumn(
            name: "exact_count",
            table: "search_request_v2");

        migrationBuilder.DropColumn(
            name: "toss_up_count",
            table: "search_request_v2");

        migrationBuilder.DropColumn(
            name: "unlikely_count",
            table: "search_request_v2");
    }
}
