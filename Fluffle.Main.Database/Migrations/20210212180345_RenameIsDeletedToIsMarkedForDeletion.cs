using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class RenameIsDeletedToIsMarkedForDeletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "content",
                newName: "is_marked_for_deletion");

            migrationBuilder.RenameIndex(
                name: "idx_content_is_deleted",
                table: "content",
                newName: "idx_content_is_marked_for_deletion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_marked_for_deletion",
                table: "content",
                newName: "is_deleted");

            migrationBuilder.RenameIndex(
                name: "idx_content_is_marked_for_deletion",
                table: "content",
                newName: "idx_content_is_deleted");
        }
    }
}
