using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class AddQueryIdToSearchRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "query_id",
                table: "search_request",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "uq_search_request_query_id",
                table: "search_request",
                column: "query_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_search_request_query_id",
                table: "search_request");

            migrationBuilder.DropColumn(
                name: "query_id",
                table: "search_request");
        }
    }
}
