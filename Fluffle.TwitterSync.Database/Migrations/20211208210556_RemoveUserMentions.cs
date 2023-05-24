using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations;

public partial class RemoveUserMentions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_tweet",
            table: "tweet_media");

        migrationBuilder.DropTable(
            name: "user_mention");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_mention",
            columns: table => new
            {
                tweet_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                user_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_mention", x => new { x.tweet_id, x.user_id });
                table.ForeignKey(
                    name: "fk_tweet",
                    column: x => x.tweet_id,
                    principalTable: "tweet",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }
}
