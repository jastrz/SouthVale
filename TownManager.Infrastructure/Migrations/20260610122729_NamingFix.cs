using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NamingFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Swordsmans",
                table: "Villages",
                newName: "Swordsmen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Swordsmen",
                table: "Villages",
                newName: "Swordsmans");
        }
    }
}
