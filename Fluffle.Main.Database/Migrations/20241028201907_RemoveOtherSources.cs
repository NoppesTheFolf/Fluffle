using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class RemoveOtherSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content",
                table: "content_other_source");

            migrationBuilder.DropTable(
                name: "content_other_source");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_other_source",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_other_source", x => x.id);
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_content_other_source_content_id",
                table: "content_other_source",
                column: "content_id");
        }
    }
}
