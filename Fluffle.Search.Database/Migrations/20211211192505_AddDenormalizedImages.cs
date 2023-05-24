using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class AddDenormalizedImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "denormalized_image",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                platform_id = table.Column<int>(type: "integer", nullable: false),
                location = table.Column<string>(type: "text", nullable: false),
                is_sfw = table.Column<bool>(type: "boolean", nullable: false),
                phash_average64 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_red256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_green256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_blue256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_average256 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_red1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_green1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_blue1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                phash_average1024 = table.Column<byte[]>(type: "bytea", nullable: false),
                thumbnail_location = table.Column<string>(type: "text", nullable: false),
                thumbnail_width = table.Column<int>(type: "integer", nullable: false),
                thumbnail_center_x = table.Column<int>(type: "integer", nullable: false),
                thumbnail_height = table.Column<int>(type: "integer", nullable: false),
                thumbnail_center_y = table.Column<int>(type: "integer", nullable: false),
                credits = table.Column<int[]>(type: "integer[]", nullable: false),
                change_id = table.Column<long>(type: "bigint", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_denormalized_image", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "uq_denormalized_image_platform_id_and_change_id",
            table: "denormalized_image",
            columns: new[] { "platform_id", "change_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "denormalized_image");
    }
}
