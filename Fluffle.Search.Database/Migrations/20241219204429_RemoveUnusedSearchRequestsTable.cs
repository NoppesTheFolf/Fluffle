using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class RemoveUnusedSearchRequestsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_request");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_request",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    area_check = table.Column<int>(type: "integer", nullable: true),
                    compare64_average = table.Column<int>(type: "integer", nullable: true),
                    complement_comparison_results = table.Column<int>(type: "integer", nullable: true),
                    compute64_average = table.Column<int>(type: "integer", nullable: true),
                    compute_expensive_blue = table.Column<int>(type: "integer", nullable: true),
                    compute_expensive_green = table.Column<int>(type: "integer", nullable: true),
                    compute_expensive_red = table.Column<int>(type: "integer", nullable: true),
                    count = table.Column<int>(type: "integer", nullable: true),
                    create_and_refine_output = table.Column<int>(type: "integer", nullable: true),
                    exception = table.Column<string>(type: "text", nullable: true),
                    flush = table.Column<int>(type: "integer", nullable: true),
                    format = table.Column<int>(type: "integer", nullable: true),
                    from = table.Column<string>(type: "text", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    link_created = table.Column<bool>(type: "boolean", nullable: true),
                    query_id = table.Column<string>(type: "text", nullable: true),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    start_expensive_rgb_computation = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    wait_for_expensive_rgb_computation = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_request", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_search_request_link_created",
                table: "search_request",
                column: "link_created");

            migrationBuilder.CreateIndex(
                name: "uq_search_request_query_id",
                table: "search_request",
                column: "query_id",
                unique: true);
        }
    }
}
