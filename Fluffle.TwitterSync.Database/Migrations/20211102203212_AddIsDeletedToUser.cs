using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations
{
    public partial class AddIsDeletedToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_user_is_furry_artist_and_is_on_e621_and_is_protected_and_is_suspended_and_reserved_until_and_followers_count",
                table: "user");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_user_is_furry_artist_and_is_on_e621_and_is_protected_and_is_suspended_and_is_deleted_and_reserved_until_and_followers_count",
                table: "user",
                columns: new[] { "is_furry_artist", "is_on_e621", "is_protected", "is_suspended", "is_deleted", "reserved_until", "followers_count" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_user_is_furry_artist_and_is_on_e621_and_is_protected_and_is_suspended_and_is_deleted_and_reserved_until_and_followers_count",
                table: "user");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user");

            migrationBuilder.CreateIndex(
                name: "idx_user_is_furry_artist_and_is_on_e621_and_is_protected_and_is_suspended_and_reserved_until_and_followers_count",
                table: "user",
                columns: new[] { "is_furry_artist", "is_on_e621", "is_protected", "is_suspended", "reserved_until", "followers_count" });
        }
    }
}
