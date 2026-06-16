using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TroopMovementAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TroopMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarriedResources_Wood = table.Column<double>(type: "double precision", nullable: true),
                    CarriedResources_Clay = table.Column<double>(type: "double precision", nullable: true),
                    CarriedResources_Iron = table.Column<double>(type: "double precision", nullable: true),
                    CarriedResources_Crop = table.Column<double>(type: "double precision", nullable: true),
                    TargetVillageId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetMapX = table.Column<int>(type: "integer", nullable: true),
                    TargetMapY = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DepartureAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivesAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VillageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Troops = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TroopMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TroopMovements_Villages_VillageId",
                        column: x => x.VillageId,
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TroopMovements_VillageId",
                table: "TroopMovements",
                column: "VillageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TroopMovements");
        }
    }
}
