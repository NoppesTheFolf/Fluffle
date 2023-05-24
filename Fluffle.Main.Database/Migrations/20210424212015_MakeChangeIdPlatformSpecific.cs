using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class MakeChangeIdPlatformSpecific : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "uq_creditable_entity_change_id",
            table: "creditable_entity");

        migrationBuilder.DropIndex(
            name: "idx_content_discriminator_and_change_id",
            table: "content");

        migrationBuilder.DropIndex(
            name: "uq_content_change_id",
            table: "content");

        migrationBuilder.CreateIndex(
            name: "uq_creditable_entity_platform_id_and_change_id",
            table: "creditable_entity",
            columns: new[] { "platform_id", "change_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "idx_content_discriminator_and_platform_id_and_change_id",
            table: "content",
            columns: new[] { "discriminator", "platform_id", "change_id" });

        migrationBuilder.CreateIndex(
            name: "uq_content_platform_id_and_change_id",
            table: "content",
            columns: new[] { "platform_id", "change_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "uq_creditable_entity_platform_id_and_change_id",
            table: "creditable_entity");

        migrationBuilder.DropIndex(
            name: "idx_content_discriminator_and_platform_id_and_change_id",
            table: "content");

        migrationBuilder.DropIndex(
            name: "uq_content_platform_id_and_change_id",
            table: "content");

        migrationBuilder.CreateIndex(
            name: "uq_creditable_entity_change_id",
            table: "creditable_entity",
            column: "change_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "idx_content_discriminator_and_change_id",
            table: "content",
            columns: new[] { "discriminator", "change_id" });

        migrationBuilder.CreateIndex(
            name: "uq_content_change_id",
            table: "content",
            column: "change_id",
            unique: true);
    }
}
