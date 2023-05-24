using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Search.Database.Migrations;

public partial class WipeData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Due to a bug found in the synchronization service, there is no certainty about the
        // correctness of the synchronized content. Deleting all the data will force the
        // synchronization process to start from the very beginning again.
        migrationBuilder.Sql("DELETE FROM content");
        migrationBuilder.Sql("DELETE FROM creditable_entity");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
