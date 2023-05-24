using Microsoft.EntityFrameworkCore.Migrations;

namespace Noppes.Fluffle.Main.Database.Migrations;

public partial class AddFurAffinityPopularArtistsView : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE VIEW fa_popular_artists AS
SELECT CE.id_on_platform as artist_id, SUM(priority) / COUNT(*) as average_score
FROM content C
JOIN content_creditable_entity CCE ON C.id = CCE.content_id
JOIN creditable_entity CE ON CCE.creditable_entity_id = CE.id
WHERE C.platform_id = 3
GROUP BY CE.id_on_platform
HAVING (COUNT(*) >= 20 AND SUM(priority) / COUNT(*) > 1000)
	OR (COUNT(*) >= 5 AND SUM(priority) / COUNT(*) > 2000)
	OR (COUNT(*) >= 3 AND SUM(priority) / COUNT(*) > 5000)
	OR (COUNT(*) >= 1 AND SUM(priority) / COUNT(*) > 8000)
ORDER BY average_score DESC;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
