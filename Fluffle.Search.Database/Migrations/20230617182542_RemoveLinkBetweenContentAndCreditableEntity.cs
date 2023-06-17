using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class RemoveLinkBetweenContentAndCreditableEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_creditable_entity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_creditable_entity",
                columns: table => new
                {
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    creditable_entity_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_creditable_entity", x => new { x.content_id, x.creditable_entity_id });
                    table.ForeignKey(
                        name: "fk_content",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_creditable_entities",
                        column: x => x.creditable_entity_id,
                        principalTable: "creditable_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_content_creditable_entity_creditable_entity_id",
                table: "content_creditable_entity",
                column: "creditable_entity_id");
        }
    }
}
