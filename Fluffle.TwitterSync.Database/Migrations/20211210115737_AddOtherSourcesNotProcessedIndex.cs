using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.TwitterSync.Database.Migrations
{
    public partial class AddOtherSourcesNotProcessedIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_other_source_has_been_processed",
                table: "other_source");

            migrationBuilder.Sql("DELETE FROM other_source;");
            migrationBuilder.Sql("CREATE INDEX idx_other_source_has_been_processed_and_id ON other_source (has_been_processed DESC, id DESC);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_other_source_has_been_processed",
                table: "other_source",
                column: "has_been_processed");
        }
    }
}
