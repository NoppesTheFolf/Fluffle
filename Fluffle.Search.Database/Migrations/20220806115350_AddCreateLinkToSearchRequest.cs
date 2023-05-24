using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class AddCreateLinkToSearchRequest : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "link_created",
            table: "search_request",
            type: "boolean",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "idx_search_request_link_created",
            table: "search_request",
            column: "link_created");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_search_request_link_created",
            table: "search_request");

        migrationBuilder.DropColumn(
            name: "link_created",
            table: "search_request");
    }
}
