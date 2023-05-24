using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class FixInvalidCreditableEntityConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_creditable_entity_platform_id",
            table: "creditable_entity");

        migrationBuilder.DropIndex(
            name: "uq_creditable_entity_id_on_platform",
            table: "creditable_entity");

        migrationBuilder.CreateIndex(
            name: "idx_creditable_entity_id_on_platform",
            table: "creditable_entity",
            column: "id_on_platform");

        migrationBuilder.CreateIndex(
            name: "idx_creditable_entity_platform_id_and_id_on_platform",
            table: "creditable_entity",
            columns: new[] { "platform_id", "id_on_platform" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_creditable_entity_id_on_platform",
            table: "creditable_entity");

        migrationBuilder.DropIndex(
            name: "idx_creditable_entity_platform_id_and_id_on_platform",
            table: "creditable_entity");

        migrationBuilder.CreateIndex(
            name: "idx_creditable_entity_platform_id",
            table: "creditable_entity",
            column: "platform_id");

        migrationBuilder.CreateIndex(
            name: "uq_creditable_entity_id_on_platform",
            table: "creditable_entity",
            column: "id_on_platform",
            unique: true);
    }
}
