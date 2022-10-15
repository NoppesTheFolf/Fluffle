using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Migrations
{
    public partial class TreatAnimatedImagesAsNormalImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE content
SET discriminator = 'Image'
WHERE media_type_id = 2;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE content
SET discriminator = 'Content'
WHERE media_type_id = 2;
");
        }
    }
}
