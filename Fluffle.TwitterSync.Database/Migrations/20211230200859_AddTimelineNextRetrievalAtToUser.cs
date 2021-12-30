using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations
{
    public partial class AddTimelineNextRetrievalAtToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "timeline_next_retrieval_at",
                table: "user",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timeline_next_retrieval_at",
                table: "user");
        }
    }
}
