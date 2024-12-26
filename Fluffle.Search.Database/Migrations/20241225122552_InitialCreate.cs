using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Noppes.Fluffle.Search.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PlatformId = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    IsSfw = table.Column<bool>(type: "boolean", nullable: false),
                    CompressedImageHashes = table.Column<byte[]>(type: "bytea", nullable: false),
                    ThumbnailLocation = table.Column<string>(type: "text", nullable: false),
                    ThumbnailWidth = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailCenterX = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailHeight = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailCenterY = table.Column<int>(type: "integer", nullable: false),
                    Credits = table.Column<int[]>(type: "integer[]", nullable: false),
                    ChangeId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    LinkCreated = table.Column<bool>(type: "boolean", nullable: true),
                    From = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    Format = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Flush = table.Column<int>(type: "integer", nullable: true),
                    AreaCheck = table.Column<int>(type: "integer", nullable: true),
                    Compute1024Red = table.Column<int>(type: "integer", nullable: true),
                    Compute1024Green = table.Column<int>(type: "integer", nullable: true),
                    Compute1024Blue = table.Column<int>(type: "integer", nullable: true),
                    Compute1024Average = table.Column<int>(type: "integer", nullable: true),
                    Compute256Red = table.Column<int>(type: "integer", nullable: true),
                    Compute256Green = table.Column<int>(type: "integer", nullable: true),
                    Compute256Blue = table.Column<int>(type: "integer", nullable: true),
                    Compute256Average = table.Column<int>(type: "integer", nullable: true),
                    Compute64Average = table.Column<int>(type: "integer", nullable: true),
                    CompareCoarse = table.Column<int>(type: "integer", nullable: true),
                    ReduceCoarseResults = table.Column<int>(type: "integer", nullable: true),
                    RetrieveImageInfo = table.Column<int>(type: "integer", nullable: true),
                    CompareGranular = table.Column<int>(type: "integer", nullable: true),
                    ReduceGranularResults = table.Column<int>(type: "integer", nullable: true),
                    CleanViewLocation = table.Column<int>(type: "integer", nullable: true),
                    RetrieveCreditableEntities = table.Column<int>(type: "integer", nullable: true),
                    AppendCreditableEntities = table.Column<int>(type: "integer", nullable: true),
                    DetermineFinalOrder = table.Column<int>(type: "integer", nullable: true),
                    LinkCreationPreparation = table.Column<int>(type: "integer", nullable: true),
                    EnqueueLinkCreation = table.Column<int>(type: "integer", nullable: true),
                    Finish = table.Column<int>(type: "integer", nullable: true),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    UnlikelyCount = table.Column<int>(type: "integer", nullable: true),
                    AlternativeCount = table.Column<int>(type: "integer", nullable: true),
                    TossUpCount = table.Column<int>(type: "integer", nullable: true),
                    ExactCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditableEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ChangeId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditableEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditableEntities_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditableEntities_PlatformId_ChangeId",
                table: "CreditableEntities",
                columns: new[] { "PlatformId", "ChangeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Images_PlatformId_ChangeId",
                table: "Images",
                columns: new[] { "PlatformId", "ChangeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_Name",
                table: "Platforms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_NormalizedName",
                table: "Platforms",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_LinkCreated",
                table: "SearchRequests",
                column: "LinkCreated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditableEntities");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "SearchRequests");

            migrationBuilder.DropTable(
                name: "Platforms");
        }
    }
}
