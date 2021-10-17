using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e621_artist",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e621_artist", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    media_type = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_furry_art = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    is_protected = table.Column<bool>(type: "boolean", nullable: false),
                    is_suspended = table.Column<bool>(type: "boolean", nullable: false),
                    followers_count = table.Column<int>(type: "integer", nullable: false),
                    is_on_e621 = table.Column<bool>(type: "boolean", nullable: false),
                    is_furry_artist = table.Column<bool>(type: "boolean", nullable: true),
                    reserved_until = table.Column<long>(type: "bigint", nullable: false),
                    timeline_retrieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "e621_artist_url",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    twitter_username = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    twitter_exists = table.Column<bool>(type: "boolean", nullable: true),
                    artist_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e621_artist_url", x => x.id);
                    table.ForeignKey(
                        name: "fk_e621_artist",
                        column: x => x.artist_id,
                        principalTable: "e621_artist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_analytic",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    furry_art = table.Column<double>(type: "double precision", nullable: false),
                    real = table.Column<double>(type: "double precision", nullable: false),
                    fursuit = table.Column<double>(type: "double precision", nullable: false),
                    anime = table.Column<double>(type: "double precision", nullable: false),
                    artist_ids = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_analytic", x => x.id);
                    table.ForeignKey(
                        name: "fk_media",
                        column: x => x.id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_size",
                columns: table => new
                {
                    media_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    resize_mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_size", x => new { x.media_id, x.size });
                    table.ForeignKey(
                        name: "fk_media",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tweet",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    favorite_count = table.Column<int>(type: "integer", nullable: false),
                    retweet_count = table.Column<int>(type: "integer", nullable: false),
                    reply_tweet_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reply_user_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    quoted_tweet_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    retweet_tweet_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_by_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    should_be_analyzed = table.Column<bool>(type: "boolean", nullable: false),
                    reserved_until = table.Column<long>(type: "bigint", nullable: false),
                    analyzed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tweet", x => x.id);
                    table.ForeignKey(
                        name: "fk_users",
                        column: x => x.created_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tweet_media",
                columns: table => new
                {
                    tweet_id = table.Column<string>(type: "character varying(20)", nullable: false),
                    media_id = table.Column<string>(type: "character varying(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tweet_media", x => new { x.media_id, x.tweet_id });
                    table.ForeignKey(
                        name: "fk_media",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tweet",
                        column: x => x.tweet_id,
                        principalTable: "tweet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "idx_e621_artist_url_artist_id",
                table: "e621_artist_url",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "idx_tweet_created_by_id",
                table: "tweet",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_tweet_quoted_tweet_id",
                table: "tweet",
                column: "quoted_tweet_id");

            migrationBuilder.CreateIndex(
                name: "idx_tweet_reply_tweet_id",
                table: "tweet",
                column: "reply_tweet_id");

            migrationBuilder.CreateIndex(
                name: "idx_tweet_reply_user_id",
                table: "tweet",
                column: "reply_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_tweet_retweet_tweet_id",
                table: "tweet",
                column: "retweet_tweet_id");

            migrationBuilder.CreateIndex(
                name: "idx_tweet_should_be_analyzed_and_analyzed_at_and_reserved_until_and_favorite_count",
                table: "tweet",
                columns: new[] { "should_be_analyzed", "analyzed_at", "reserved_until", "favorite_count" });

            migrationBuilder.CreateIndex(
                name: "idx_tweet_media_tweet_id",
                table: "tweet_media",
                column: "tweet_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_is_furry_artist_and_is_on_e621_and_is_protected_and_is_suspended_and_reserved_until_and_followers_count",
                table: "user",
                columns: new[] { "is_furry_artist", "is_on_e621", "is_protected", "is_suspended", "reserved_until", "followers_count" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "e621_artist_url");

            migrationBuilder.DropTable(
                name: "media_analytic");

            migrationBuilder.DropTable(
                name: "media_size");

            migrationBuilder.DropTable(
                name: "tweet_media");

            migrationBuilder.DropTable(
                name: "user_mention");

            migrationBuilder.DropTable(
                name: "e621_artist");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "tweet");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
