using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class AddB2FileIdToAndFilenameThumbnail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "b2_file_id",
                table: "thumbnail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "filename",
                table: "thumbnail",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_thumbnail_b2_file_id",
                table: "thumbnail",
                column: "b2_file_id");

            migrationBuilder.CreateIndex(
                name: "idx_thumbnail_filename",
                table: "thumbnail",
                column: "filename");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_thumbnail_b2_file_id",
                table: "thumbnail");

            migrationBuilder.DropIndex(
                name: "idx_thumbnail_filename",
                table: "thumbnail");

            migrationBuilder.DropColumn(
                name: "b2_file_id",
                table: "thumbnail");

            migrationBuilder.DropColumn(
                name: "filename",
                table: "thumbnail");
        }
    }
}
