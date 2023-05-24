using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations;

public partial class AddIndexToSpeedUpSupplyingExistingUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "idx_user_is_furry_artist_and_is_protected_and_is_suspended_and_is_deleted_and_reserved_until_and_timeline_next_retrieval_at",
            table: "user",
            columns: new[] { "is_furry_artist", "is_protected", "is_suspended", "is_deleted", "reserved_until", "timeline_next_retrieval_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_user_is_furry_artist_and_is_protected_and_is_suspended_and_is_deleted_and_reserved_until_and_timeline_next_retrieval_at",
            table: "user");
    }
}
