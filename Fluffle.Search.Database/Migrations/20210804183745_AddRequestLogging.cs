using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class AddRequestLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_request",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    exception = table.Column<string>(type: "text", nullable: true),
                    format = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: true),
                    flush = table.Column<int>(type: "integer", nullable: true),
                    area_check = table.Column<int>(type: "integer", nullable: true),
                    start256_rgb_computation = table.Column<int>(type: "integer", nullable: true),
                    compute256_red = table.Column<int>(type: "integer", nullable: true),
                    compute256_green = table.Column<int>(type: "integer", nullable: true),
                    compute256_blue = table.Column<int>(type: "integer", nullable: true),
                    compute64_average = table.Column<int>(type: "integer", nullable: true),
                    compare64_average = table.Column<int>(type: "integer", nullable: true),
                    complement_comparison_results = table.Column<int>(type: "integer", nullable: true),
                    wait_for256_rgb_computation = table.Column<int>(type: "integer", nullable: true),
                    create_and_refine_output = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_request", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_request");
        }
    }
}
