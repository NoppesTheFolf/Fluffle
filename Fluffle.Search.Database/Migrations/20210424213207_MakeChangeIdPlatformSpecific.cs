using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class MakeChangeIdPlatformSpecific : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All of the previously retrieved creditable entities didn't contain the ID of the
            // platform they belonged to. Therefore all of that data is now useless.
            migrationBuilder.Sql("DELETE FROM content");
            migrationBuilder.Sql("DELETE FROM creditable_entity");

            migrationBuilder.DropIndex(
                name: "uq_creditable_entity_change_id",
                table: "creditable_entity");

            migrationBuilder.DropIndex(
                name: "idx_content_platform_id",
                table: "content");

            migrationBuilder.DropIndex(
                name: "uq_content_change_id",
                table: "content");

            migrationBuilder.AddColumn<int>(
                name: "platform_id",
                table: "creditable_entity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_creditable_entity_platform_id_and_change_id",
                table: "creditable_entity",
                columns: new[] { "platform_id", "change_id" });

            migrationBuilder.CreateIndex(
                name: "uq_content_platform_id_and_change_id",
                table: "content",
                columns: new[] { "platform_id", "change_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_platform",
                table: "creditable_entity",
                column: "platform_id",
                principalTable: "platform",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_platform",
                table: "content");

            migrationBuilder.DropIndex(
                name: "idx_creditable_entity_platform_id_and_change_id",
                table: "creditable_entity");

            migrationBuilder.DropIndex(
                name: "uq_content_platform_id_and_change_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "platform_id",
                table: "creditable_entity");

            migrationBuilder.CreateIndex(
                name: "uq_creditable_entity_change_id",
                table: "creditable_entity",
                column: "change_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_content_platform_id",
                table: "content",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "uq_content_change_id",
                table: "content",
                column: "change_id",
                unique: true);
        }
    }
}
