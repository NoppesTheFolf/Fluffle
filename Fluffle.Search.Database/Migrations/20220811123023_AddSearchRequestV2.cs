using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class AddSearchRequestV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_request_v2",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    link_created = table.Column<bool>(type: "boolean", nullable: true),
                    from = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    exception = table.Column<string>(type: "text", nullable: true),
                    format = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    flush = table.Column<int>(type: "integer", nullable: true),
                    area_check = table.Column<int>(type: "integer", nullable: true),
                    compute1024_red = table.Column<int>(type: "integer", nullable: true),
                    compute1024_green = table.Column<int>(type: "integer", nullable: true),
                    compute1024_blue = table.Column<int>(type: "integer", nullable: true),
                    compute1024_average = table.Column<int>(type: "integer", nullable: true),
                    compute256_red = table.Column<int>(type: "integer", nullable: true),
                    compute256_green = table.Column<int>(type: "integer", nullable: true),
                    compute256_blue = table.Column<int>(type: "integer", nullable: true),
                    compute256_average = table.Column<int>(type: "integer", nullable: true),
                    compute64_average = table.Column<int>(type: "integer", nullable: true),
                    compare_coarse = table.Column<int>(type: "integer", nullable: true),
                    reduce_coarse_results = table.Column<int>(type: "integer", nullable: true),
                    retrieve_image_info = table.Column<int>(type: "integer", nullable: true),
                    compare_granular = table.Column<int>(type: "integer", nullable: true),
                    reduce_granular_results = table.Column<int>(type: "integer", nullable: true),
                    clean_view_location = table.Column<int>(type: "integer", nullable: true),
                    retrieve_creditable_entities = table.Column<int>(type: "integer", nullable: true),
                    append_creditable_entities = table.Column<int>(type: "integer", nullable: true),
                    determine_final_order = table.Column<int>(type: "integer", nullable: true),
                    link_creation_preparation = table.Column<int>(type: "integer", nullable: true),
                    enqueue_link_creation = table.Column<int>(type: "integer", nullable: true),
                    count = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_request_v2", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_search_request_v2_link_created",
                table: "search_request_v2",
                column: "link_created");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_request_v2");
        }
    }
}
