using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCropToBeer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Crop",
                table: "Villages",
                newName: "Beer");

            migrationBuilder.RenameColumn(
                name: "CarriedResources_Crop",
                table: "TroopMovements",
                newName: "CarriedResources_Beer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Beer",
                table: "Villages",
                newName: "Crop");

            migrationBuilder.RenameColumn(
                name: "CarriedResources_Beer",
                table: "TroopMovements",
                newName: "CarriedResources_Crop");
        }
    }
}
