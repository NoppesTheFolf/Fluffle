using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class AddCreditableEntityPriority : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "priority",
            table: "creditable_entity",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "priority_updated_at",
            table: "creditable_entity",
            type: "timestamp without time zone",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "idx_creditable_entity_priority_updated_at",
            table: "creditable_entity",
            column: "priority_updated_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_creditable_entity_priority_updated_at",
            table: "creditable_entity");

        migrationBuilder.DropColumn(
            name: "priority",
            table: "creditable_entity");

        migrationBuilder.DropColumn(
            name: "priority_updated_at",
            table: "creditable_entity");
    }
}
