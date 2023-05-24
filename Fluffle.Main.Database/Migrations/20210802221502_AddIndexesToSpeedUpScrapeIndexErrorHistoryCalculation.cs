using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class AddIndexesToSpeedUpScrapeIndexErrorHistoryCalculation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "idx_image_hash_created_at",
            table: "image_hash",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "idx_content_error_created_at",
            table: "content_error",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "idx_content_created_at_and_platform_id",
            table: "content",
            columns: new[] { "created_at", "platform_id" });

        migrationBuilder.CreateIndex(
            name: "idx_content_discriminator_and_id_and_platform_id",
            table: "content",
            columns: new[] { "discriminator", "id", "platform_id" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_image_hash_created_at",
            table: "image_hash");

        migrationBuilder.DropIndex(
            name: "idx_content_error_created_at",
            table: "content_error");

        migrationBuilder.DropIndex(
            name: "idx_content_created_at_and_platform_id",
            table: "content");

        migrationBuilder.DropIndex(
            name: "idx_content_discriminator_and_id_and_platform_id",
            table: "content");
    }
}
