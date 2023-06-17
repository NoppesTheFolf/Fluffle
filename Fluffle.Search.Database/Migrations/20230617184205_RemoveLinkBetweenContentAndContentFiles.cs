using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class RemoveLinkBetweenContentAndContentFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content",
                table: "content_file");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_content",
                table: "content_file",
                column: "content_id",
                principalTable: "content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
