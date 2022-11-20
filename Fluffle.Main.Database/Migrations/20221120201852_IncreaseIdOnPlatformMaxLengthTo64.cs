using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class IncreaseIdOnPlatformMaxLengthTo64 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_image_ratings",
                table: "content");

            migrationBuilder.AlterColumn<string>(
                name: "id_on_platform",
                table: "content",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AddForeignKey(
                name: "fk_content_ratings",
                table: "content",
                column: "rating_id",
                principalTable: "content_rating",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content_ratings",
                table: "content");

            migrationBuilder.AlterColumn<string>(
                name: "id_on_platform",
                table: "content",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddForeignKey(
                name: "fk_image_ratings",
                table: "content",
                column: "rating_id",
                principalTable: "content_rating",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
