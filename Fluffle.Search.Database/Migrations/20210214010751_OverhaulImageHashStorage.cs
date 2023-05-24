using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class OverhaulImageHashStorage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "phash_average1024",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_average256",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_average64",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_blue1024",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_blue256",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_blue64",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_green1024",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_green256",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_green64",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_red1024",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_red256",
            table: "content");

        migrationBuilder.DropColumn(
            name: "phash_red64",
            table: "content");

        migrationBuilder.CreateTable(
            name: "image_hash",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                phash_red64 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_green64 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_blue64 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_average64 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_red256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_green256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_blue256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_average256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_red1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_green1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_blue1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_average1024 = table.Column<byte[]>(type: "bytea", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_image_hash", x => x.id);
                table.ForeignKey(
                    name: "fk_content",
                    column: x => x.id,
                    principalTable: "content",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("DELETE FROM content;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "image_hash");

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_average1024",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_average256",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_average64",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_blue1024",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_blue256",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_blue64",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_green1024",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_green256",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_green64",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_red1024",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_red256",
            table: "content",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "phash_red64",
            table: "content",
            type: "bytea",
            nullable: true);
    }
}
