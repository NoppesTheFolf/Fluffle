using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddIdAndPriorityIndexToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_content_id_and_priority",
                table: "content",
                columns: new[] { "id", "priority" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_content_id_and_priority",
                table: "content");
        }
    }
}
